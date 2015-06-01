using OpenAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Zelda.Game.Engine
{
    class Sound : DisposableObject
    {
        [StructLayout(LayoutKind.Sequential)]
        public class SoundFromMemory
        {
            public MemoryStream data;
            public uint position;
            public bool loop;
        }

        public static readonly Vorbisfile.ov_callbacks OggCallbacks;

        static IntPtr _device;
        static IntPtr _context;
        
        static float _volume = 1.0f;
        public static float Volume
        {
            get { return _volume; }
            set
            {
                value = Math.Min(100, Math.Max(0, value));
                _volume = value / 100.0f;
            }
        }

        static bool _initialized;
        public static bool IsInitialized
        {
            get { return _initialized; }
        }

        static Sound()
        {
            OggCallbacks = new Vorbisfile.ov_callbacks();
            OggCallbacks.read_func = CbRead;
            OggCallbacks.seek_func = null;
            OggCallbacks.close_func = null;
            OggCallbacks.tell_func = null;
        }

        public static void Initialize(Arguments args)
        {
            bool disable = args.HasArgument("-no-audio");
            if (disable)
                return;

            // OpenAL 초기화
            _device = ALC10.alcOpenDevice(null);
            if (_device == IntPtr.Zero)
            {
                Debug.Error("Cannot open audio device");
                return;
            }

            int[] attr = { ALC10.ALC_FREQUENCY, 32000, 0 }; // SPC 출력 샘플링 레이트가 32 KHZ
            _context = ALC10.alcCreateContext(_device, attr);
            if (_context == IntPtr.Zero)
            {
                Debug.Error("Cannot create audio context");
                ALC10.alcCloseDevice(_device);
                return;
            }
            if (!ALC10.alcMakeContextCurrent(_context))
            {
                Debug.Error("Cannot activate audio context");
                ALC10.alcDestroyContext(_context);
                ALC10.alcCloseDevice(_device);
                return;
            }

            AL10.alGenBuffers(IntPtr.Zero, null);   // 몇몇 시스템에서 첫 사운드가 로드될 때 발생하는 에러 대비

            _initialized = true;
            Volume = 100;

            Music.Initialize();
        }

        public static void Quit()
        {
            if (!IsInitialized)
                return;

            Music.Quit();

            _allSounds.Clear();

            // OpenAL 정리
            ALC10.alcMakeContextCurrent(IntPtr.Zero);
            ALC10.alcDestroyContext(_context);
            _context = IntPtr.Zero;
            ALC10.alcCloseDevice(_device);
            _device = IntPtr.Zero;
            
            _initialized = false;
        }

        public static void Update()
        {
            List<Sound> soundsToRemove = new List<Sound>();
            foreach (Sound sound in _currentSounds)
            {
                if (!sound.UpdatePlaying())
                    soundsToRemove.Add(sound);
            }

            foreach (Sound sound in soundsToRemove)
                _currentSounds.Remove(sound);

            Music.Update();
        }

        public static uint CbRead(IntPtr ptr, uint size, uint nbBytes, IntPtr datasource)
        {
            GCHandle handle = GCHandle.FromIntPtr(datasource);
            SoundFromMemory mem = (SoundFromMemory)handle.Target;

            uint totalSize = (uint)mem.data.Length;
            if (mem.position >= totalSize)
            {
                if (mem.loop)
                    mem.position = 0;
                else
                    return 0;
            }
            else if (mem.position + nbBytes >= totalSize)
                nbBytes = totalSize - mem.position;

            Marshal.Copy(mem.data.GetBuffer(), (int)mem.position, ptr, (int)nbBytes);
            mem.position += nbBytes;

            return nbBytes;
        }

        static readonly List<Sound> _currentSounds = new List<Sound>();
        static readonly Dictionary<string, Sound> _allSounds = new Dictionary<string, Sound>();

        public static bool Exists(string soundId)
        {
            return ModFiles.DataFileExists("sounds/{0}.ogg".F(soundId));
        }

        public static void Play(string soundId)
        {
            if (!_allSounds.ContainsKey(soundId))
                _allSounds.Add(soundId, new Sound(soundId));

            _allSounds[soundId].Start();
        }

        readonly string _id;
        uint _buffer = AL10.AL_NONE;
        Queue<uint> _sources = new Queue<uint>();
        
        public Sound(string soundId = "")
        {
            _id = soundId;
        }

        protected override void OnDispose(bool disposing)
        {
            if (IsInitialized && _buffer != AL10.AL_NONE)
            {
                foreach (uint source in _sources)
                {
                    AL10.alSourceStop(source);
                    AL10.alSourcei(source, AL10.AL_BUFFER, 0);
                    uint sourceToDelete = source;
                    AL10.alDeleteSources((IntPtr)1, ref sourceToDelete);
                }
                AL10.alDeleteBuffers((IntPtr)1, ref _buffer);
                _currentSounds.Remove(this);
            }
        }

        public void Load()
        {
            if (AL10.alGetError() != AL10.AL_NONE)
                Debug.Error("Previous audio error not cleaned");

            string fileName = "sounds/" + _id;
            if (!_id.Contains("."))
                fileName += ".ogg";

            // 디코딩된 사운드로 OpenAL 퍼버를 생성합니다
            _buffer = DecodeFile(fileName);

            // 실패한 경우 _buffer는 AL_NONE인 상태
        }

        public bool Start()
        {
            bool success = false;
            
            if (!IsInitialized)
                return success;

            if (_buffer == AL10.AL_NONE)
            {
                Load();
                if (_buffer == AL10.AL_NONE)
                    return success;
            }

            // 소스를 생성합니다
            uint source;
            AL10.alGenSources((IntPtr)1, out source);
            AL10.alSourcei(source, AL10.AL_BUFFER, (int)_buffer);
            AL10.alSourcef(source, AL10.AL_GAIN, _volume);

            // 사운드를 재생합니다
            int error = AL10.alGetError();
            if (error != AL10.AL_NO_ERROR)
            {
                Debug.Error("Cannot attach buffer {0} to the source to play sound '{1}': error {2}"
                    .F(_buffer, _id, error));
                AL10.alDeleteSources((IntPtr)1, ref source);
            }
            else
            {
                _sources.Enqueue(source);
                _currentSounds.Remove(this);    // 중복을 막기 위해
                _currentSounds.Add(this);
                AL10.alSourcePlay(source);
                error = AL10.alGetError();
                if (error != AL10.AL_NO_ERROR)
                    Debug.Error("Cannot play sound '{0}': error {1}".F(_id, error));
                else
                    success = true;
            }
            return success;
        }

        bool UpdatePlaying()
        {
            if (_sources.Count <= 0)
                return false;

            uint source = _sources.Peek();
            int status;
            AL10.alGetSourcei(source, AL10.AL_SOURCE_STATE, out status);

            if (status != AL10.AL_PLAYING)
            {
                _sources.Dequeue();
                AL10.alSourcei(source, AL10.AL_BUFFER, 0);
                AL10.alDeleteSources((IntPtr)1, ref source);
            }
            return (_sources.Count > 0);
        }

        uint DecodeFile(string fileName)
        {
            uint buffer = AL10.AL_NONE;

            if (!ModFiles.DataFileExists(fileName))
            {
                Debug.Error("Cannot find sound file '{0}'".F(fileName));
                return AL10.AL_NONE;
            }

            // 사운드 파일을 읽어들입니다
            SoundFromMemory mem = new SoundFromMemory();
            mem.loop = false;
            mem.position = 0;
            mem.data = ModFiles.DataFileRead(fileName);
            GCHandle memHandle = GCHandle.Alloc(mem);

            IntPtr file;
            int error = Vorbisfile.ov_open_callback(GCHandle.ToIntPtr(memHandle), out file, IntPtr.Zero, 0, Sound.OggCallbacks);

            if (error != 0)
                Debug.Error("Cannot load sound file '{0}' from memory: error {1}".F(fileName, error));
            else
            {
                // 인코딩된 사운드의 속성을 읽습니다
                Vorbisfile.vorbis_info info = Vorbisfile.ov_info(file, -1);
                int sampleRate = info.rate;

                int format = AL10.AL_NONE;
                if (info.channels == 1)
                    format = AL10.AL_FORMAT_MONO16;
                else if (info.channels == 2)
                    format = AL10.AL_FORMAT_STEREO16;

                if (format == AL10.AL_NONE)
                    Debug.Error("Invalid audio format for sound file '{0}'".F(fileName));
                else
                {
                    // vorbisfile로 디코딩
                    MemoryStream samples = new MemoryStream();
                    int bitstream;
                    int bytesRead;
                    int totalBytesRead = 0;
                    int bufferSize = 4096;
                    byte[] samplesBuffer = new byte[bufferSize];
                    do
                    {
                        bytesRead = Vorbisfile.ov_read(file, samplesBuffer, bufferSize, 0, 2, 1, out bitstream);
                        if (bytesRead < 0)
                            Debug.Error("Error while decoding ogg chunk in sound file '{0}': {1}".F(fileName, bytesRead));
                        else
                        {
                            totalBytesRead += bytesRead;
                            if (format == AL10.AL_FORMAT_STEREO16)
                                samples.Write(samplesBuffer, 0, bytesRead);
                            else
                            {
                                // 런타임에 스테레오 사운드로 변경
                                // TODO: 더 나은 방법 찾기
                                for (int i = 0; i < bytesRead; i += 2)
                                {
                                    samples.Write(samplesBuffer, i, 2);
                                    samples.Write(samplesBuffer, i, 2);
                                }
                                totalBytesRead += bytesRead;
                            }
                        }
                    }
                    while (bytesRead > 0);

                    // 샘플을 OpenAL 퍼버로 복사합니다
                    AL10.alGenBuffers((IntPtr)1, out buffer);
                    if (AL10.alGetError() != AL10.AL_NO_ERROR)
                        Debug.Error("Failed to generate audio buffer");
                    AL10.alBufferData(buffer, AL10.AL_FORMAT_STEREO16, samples.GetBuffer(), (IntPtr)totalBytesRead, (IntPtr)sampleRate);
                    error = AL10.alGetError();
                    if (error != AL10.AL_NO_ERROR)
                    {
                        Debug.Error("Cannot copy the sound samples of '{0}' into buffer {1}: error {2}"
                            .F(fileName, buffer, error));
                        buffer = AL10.AL_NONE;
                    }
                }
                Vorbisfile.ov_clear(ref file);
            }

            memHandle.Free();
            mem.data.Dispose();
            return buffer;
        }
    }
}
