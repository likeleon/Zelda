using OpenAL;
using System;
using System.Runtime.InteropServices;

namespace Zelda.Game.Engine
{
    class Music
    {
        public enum Format
        {
            NoFormat,
            Spc,
            It,
            Ogg
        }

        #region Static
        public static readonly string None = "None";
        public static readonly string Unchanged = "Same";

        static readonly int _nbBuffers = 8;
        static uint[] _buffers = new uint[_nbBuffers];

        static bool _initialized;
        public static bool IsInitialized
        {
            get { return _initialized; }
        }

        static float _volume = 1.0f;
        
        static Music _currentMusic;
        public static string CurrentMusicId
        {
            get { return (_currentMusic != null) ? _currentMusic._id : None; }
        }

        public static Format CurrentFormat
        {
            get
            {
                if (_currentMusic == null)
                    return Format.NoFormat;
                
                return _currentMusic._format;
            }
        }

        public static int Volume
        {
            get { return (int)(_volume * 100.0 + 0.5); }
            set
            {
                value = Math.Min(100, Math.Max(0, value));
                _volume = value / 100.0f;

                if (_currentMusic != null)
                    AL10.alSourcef(_currentMusic._source, AL10.AL_GAIN, _volume);
            }
        }

        public static void Initialize()
        {
            Volume = 100;
            _initialized = true;
        }

        public static void Quit()
        {
            if (IsInitialized)
            {
                _currentMusic = null;
            }
        }

        public static void Update()
        {
            if (!IsInitialized)
                return;

            if (_currentMusic != null)
            {
                bool playing = _currentMusic.UpdatePlaying();
                if (!playing)
                    _currentMusic = null;
            }
        }

        public static void Play(string musicId, bool loop)
        {
            if (musicId == Unchanged || musicId == CurrentMusicId)
                return;

            if (_currentMusic != null)
            {
                _currentMusic.Stop();
                _currentMusic = null;
            }

            if (musicId != None)
            {
                _currentMusic = new Music(musicId, loop);
                if (!_currentMusic.Start())
                    _currentMusic = null;
            }
        }

        public static void FindMusicFile(string musicId, ref string fileName, ref Format format)
        {
            fileName = null;
            format = Format.Ogg;

            string fileNameStart = "musics/" + musicId;
            if (ModFiles.DataFileExists(fileNameStart + ".ogg"))
            {
                format = Format.Ogg;
                fileName = fileNameStart + ".ogg";
            }
        }
        #endregion

        #region Instance
        readonly string _id;
        Format _format;
        readonly bool _loop;
        uint _source = AL10.AL_NONE;
        string _fileName;

        // OGG 한정
        IntPtr _oggFile;
        readonly Sound.SoundFromMemory _oggMem = new Sound.SoundFromMemory();
        GCHandle _oggMemHandle;

        Music(string musicId, bool loop)
        {
            _id = musicId;
            _format = Format.Ogg;
            _loop = loop;

            for (int i = 0; i < _nbBuffers; ++i)
                _buffers[i] = AL10.AL_NONE;
        }

        bool Start()
        {
            if (!IsInitialized)
                return false;

            if (_fileName == null)
            {
                FindMusicFile(_id, ref _fileName, ref _format);
                if (_fileName == null)
                {
                    Debug.Error("Cannot find music file 'musics/{0}' (tried with extentions .ogg)".F(_id));
                    return false;
                }
            }

            bool success = true;

            // 버퍼와 소스를 만듭니다
            AL10.alGenBuffers((IntPtr)_nbBuffers, _buffers);
            AL10.alGenSources((IntPtr)1, out _source);
            AL10.alSourcef(_source, AL10.AL_GAIN, _volume);

            // 음악을 메모리로 읽어옵니다
            switch (_format)
            {
                case Format.Ogg:
                {
                    _oggMem.position = 0;
                    _oggMem.loop = _loop;
                    _oggMem.data = ModFiles.DataFileRead(_fileName);
                    _oggMemHandle = GCHandle.Alloc(_oggMem);
                    // 이제 _oggmem은 인코딩된 데이터를 가집니다

                    int error = Vorbisfile.ov_open_callback(GCHandle.ToIntPtr(_oggMemHandle), out _oggFile, IntPtr.Zero, 0, Sound.OggCallbacks);
                    if (error != 0)
                        Debug.Error("Cannot load music file '{0}' from memory: error {1}".F(_fileName, error));
                    else
                    {
                        for (int i = 0; i < _nbBuffers; ++i)
                            DecodeOgg(_buffers[i], 4096);
                    }
                    break;
                }

                default:
                    Debug.Die("Invalid music format");
                    break;
            }

            // 스트리밍을 시작합니다
            AL10.alSourceQueueBuffers(_source, (IntPtr)_nbBuffers, _buffers);
            int alError = AL10.alGetError();
            if (alError != AL10.AL_NO_ERROR)
            {
                string msg = "Cannot initialize buffers for music '{0}': error {1}".F(_fileName, alError);
                Debug.Error(msg);
                success = false;
            }

            AL10.alSourcePlay(_source);
            
            return success;
        }
        
        void Stop()
        {
            if (!IsInitialized)
                return;

            int nbQueued;
            uint buffer = 0;
            AL10.alGetSourcei(_source, AL10.AL_BUFFERS_QUEUED, out nbQueued);
            for (int i = 0; i < nbQueued; ++i)
                AL10.alSourceUnqueueBuffers(_source, (IntPtr)1, ref buffer);
            AL10.alSourcei(_source, AL10.AL_BUFFER, 0);

            // 소스를 삭제합니다
            AL10.alDeleteSources((IntPtr)1, ref _source);

            // 퍼버들을 삭제합니다
            AL10.alDeleteBuffers((IntPtr)_nbBuffers, _buffers);

            switch (_format)
            {
                case Format.Ogg:
                    Vorbisfile.ov_clear(ref _oggFile);
                    _oggMemHandle.Free();
                    _oggMem.data.Dispose();
                    break;

                default:
                    Debug.Die("Invalid music format");
                    break;
            }
        }

        bool UpdatePlaying()
        {
            // 빈 버퍼를 얻습니다
            int nbEmpty;
            AL10.alGetSourcei(_source, AL10.AL_BUFFERS_PROCESSED, out nbEmpty);

            // 다시 채워줍니다
            for (int i = 0; i < nbEmpty; ++i)
            {
                uint buffer = 0;
                AL10.alSourceUnqueueBuffers(_source, (IntPtr)1, ref buffer);    // 큐에서 제거

                // 디코딩한 데이터로 채워줍니다
                switch (_format)
                {
                    case Format.Ogg:
                        DecodeOgg(buffer, 4096);
                        break;

                    default:
                        Debug.Die("Invalid music format");
                        break;
                }

                AL10.alSourceQueueBuffers(_source, (IntPtr)1, ref buffer);  // 큐에 추가
            }

            // 여전히 재생할 것들이 남아있는지 확인합니다
            int status;
            AL10.alGetSourcei(_source, AL10.AL_SOURCE_STATE, out status);
            if (status != AL10.AL_PLAYING)
            {
                // EOF를 만났거나, 데이터를 더 디코딩해야 하는 경우입니다
                AL10.alSourcePlay(_source);
            }

            AL10.alGetSourcei(_source, AL10.AL_SOURCE_STATE, out status);
            return (status == AL10.AL_PLAYING);
        }

        void DecodeOgg(uint destinationBuffer, int nbSamples)
        {
            // 인코딩된 음악의 속성을 얻습니다
            Vorbisfile.vorbis_info info = Vorbisfile.ov_info(_oggFile, -1);
            int sampleRate = info.rate;
            
            int alFormat = AL10.AL_NONE;
            if (info.channels == 1)
                alFormat = AL10.AL_FORMAT_MONO16;
            else if (info.channels == 2)
                alFormat = AL10.AL_FORMAT_STEREO16;

            // OGG 데이터 디코딩
            byte[] rawData = new byte[nbSamples * info.channels];
            int bitstream;
            int bytesRead;
            int totalBytesRead = 0;
            int remainingBytes = rawData.Length;
            do
            {
                GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
                IntPtr buffer = handle.AddrOfPinnedObject();
                bytesRead = Vorbisfile.ov_read(_oggFile, IntPtr.Add(buffer, totalBytesRead), remainingBytes, 0, 2, 1, out bitstream);
                handle.Free();
                if (bytesRead < 0)
                {
                    if (bytesRead != Vorbisfile.OV_HOLE) // 루프타입일 경우 OV_HOLE은 에러가 아닙니다
                    {
                        Debug.Error("Error while decoding ogg chunk: {0}".F(bytesRead));
                        return;
                    }
                }
                else
                {
                    totalBytesRead += bytesRead;
                    remainingBytes -= bytesRead;
                }
            }
            while (remainingBytes > 0 && bytesRead > 0);

            // 디코딩된 PCM데이터를 버퍼에 넣습니다
            AL10.alBufferData(destinationBuffer, alFormat, rawData, (IntPtr)totalBytesRead, (IntPtr)sampleRate);

            int error = AL10.alGetError();
            if (error != AL10.AL_NO_ERROR)
            {
                string msg = "Failed to fill the audio buffer with decoded OGG data for music file '{0}': error {1}"
                    .F(_fileName, error);
                Debug.Error(msg);
            }
        }
        #endregion
    }
}
