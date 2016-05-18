using OpenAL;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Zelda.Game.LowLevel
{
    public enum MusicFormat
    {
        NoFormat,
        Spc,
        It,
        Ogg
    }

    class Music
    {
        #region Static
        public static readonly string None = "None";
        public static readonly string Unchanged = "Same";

        static readonly int _numBuffers = 8;
        static uint[] _buffers = new uint[_numBuffers];
        static ItDecoder _itDecoder;
        static float _volume = 1.0f;
        static Music _currentMusic;

        public static bool IsInitialized { get; private set; }

        public static string CurrentMusicId { get { return (_currentMusic != null) ? _currentMusic._id : None; } }

        public static MusicFormat Format
        {
            get
            {
                if (_currentMusic == null)
                    return MusicFormat.NoFormat;

                return _currentMusic._format;
            }
        }

        public static int Volume { get { return (int)(_volume * 100.0 + 0.5); } }

        public static void Initialize()
        {
            _itDecoder = new ItDecoder();

            SetVolume(100);
            IsInitialized = true;
        }

        public static void Quit()
        {
            if (IsInitialized)
            {
                _currentMusic = null;
                _itDecoder = null;
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
                {
                    var callback = _currentMusic._callback;
                    _currentMusic = null;
                    if (callback != null)
                        callback.Invoke();
                }
            }
        }

        public static bool Exists(string musicId)
        {
            if (musicId == None || musicId == Unchanged)
                return true;

            string fileName;
            MusicFormat format;
            FindMusicFile(musicId, out fileName, out format);
            return !String.IsNullOrEmpty(fileName);
        }

        public static void Play(string musicId, bool loop, Action callback = null)
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
                _currentMusic = new Music(musicId, loop, callback);
                if (!_currentMusic.Start())
                    _currentMusic = null;
            }
        }

        public static void StopPlaying()
        {
            Play(None, false);
        }

        public static void FindMusicFile(string musicId, out string fileName, out MusicFormat format)
        {
            fileName = null;
            format = MusicFormat.Ogg;

            string fileNameStart = "musics/" + musicId;
            if (MainLoop.Mod.ModFiles.DataFileExists(fileNameStart + ".ogg"))
            {
                format = MusicFormat.Ogg;
                fileName = fileNameStart + ".ogg";
            }
            else if (MainLoop.Mod.ModFiles.DataFileExists(fileNameStart + ".it"))
            {
                format = MusicFormat.It;
                fileName = fileNameStart + ".it";
            }
        }

        public static void SetVolume(int volume)
        {
            volume = Math.Min(100, Math.Max(0, volume));
            _volume = volume / 100.0f;

            if (_currentMusic != null)
                AL10.alSourcef(_currentMusic._source, AL10.AL_GAIN, _volume);
        }

        public static int GetNumChannels()
        {
            Debug.CheckAssertion(Format == MusicFormat.It,
                "This function is only supported for .it musics");

            return _itDecoder.GetNumChannels();
        }

        public static int GetChannelVolume(int channel)
        {
            Debug.CheckAssertion(Format == MusicFormat.It,
                "This function is only supported for .it musics");

            return _itDecoder.GetChannelVolume(channel);
        }

        public static void SetChannelVolume(int channel, int volume)
        {
            Debug.CheckAssertion(Format == MusicFormat.It,
                "This function is only supported for .it musics");

            _itDecoder.SetChannelVolume(channel, volume);
        }
        #endregion

        #region Instance
        readonly string _id;
        readonly bool _loop;

        MusicFormat _format;
        uint _source = AL10.AL_NONE;
        string _fileName;
        Action _callback;

        // OGG 한정
        IntPtr _oggFile;
        readonly Sound.SoundFromMemory _oggMem = new Sound.SoundFromMemory();
        GCHandle _oggMemHandle;

        Music(string musicId, bool loop, Action callback)
        {
            _id = musicId;
            _format = MusicFormat.Ogg;
            _loop = loop;
            _callback = callback;

            Debug.CheckAssertion(!loop || callback == null,
                "Attemp to set both a loop and a callback to music");

            for (int i = 0; i < _numBuffers; ++i)
                _buffers[i] = AL10.AL_NONE;
        }

        bool Start()
        {
            if (!IsInitialized)
                return false;

            if (_fileName == null)
            {
                FindMusicFile(_id, out _fileName, out _format);
                if (_fileName == null)
                {
                    Debug.Error("Cannot find music file 'musics/{0}' (tried with extentions .ogg)".F(_id));
                    return false;
                }
            }

            bool success = true;

            // 버퍼와 소스를 만듭니다
            AL10.alGenBuffers((IntPtr)_numBuffers, _buffers);
            AL10.alGenSources((IntPtr)1, out _source);
            AL10.alSourcef(_source, AL10.AL_GAIN, _volume);

            // 음악을 메모리로 읽어옵니다
            byte[] soundBuffer;

            switch (_format)
            {
                case MusicFormat.It:
                    {
                        soundBuffer = MainLoop.Mod.ModFiles.DataFileRead(_fileName);

                        _itDecoder.Load(soundBuffer);

                        for (int i = 0; i < _numBuffers; ++i)
                            DecodeIt(_buffers[i], 4096);
                        break;
                    }

                case MusicFormat.Ogg:
                    {
                        _oggMem.position = 0;
                        _oggMem.loop = _loop;
                        _oggMem.data = MainLoop.Mod.ModFiles.DataFileRead(_fileName);
                        _oggMemHandle = GCHandle.Alloc(_oggMem);
                        // 이제 _oggmem은 인코딩된 데이터를 가집니다

                        int error = Vorbisfile.ov_open_callback(GCHandle.ToIntPtr(_oggMemHandle), out _oggFile, IntPtr.Zero, 0, Sound.OggCallbacks);
                        if (error != 0)
                            Debug.Error("Cannot load music file '{0}' from memory: error {1}".F(_fileName, error));
                        else
                        {
                            for (int i = 0; i < _numBuffers; ++i)
                                DecodeOgg(_buffers[i], 4096);
                        }
                        break;
                    }

                default:
                    Debug.Die("Invalid music format");
                    break;
            }

            // 스트리밍을 시작합니다
            AL10.alSourceQueueBuffers(_source, (IntPtr)_numBuffers, _buffers);
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

            _callback = null;

            AL10.alSourceStop(_source);

            int nbQueued;
            uint buffer = 0;
            AL10.alGetSourcei(_source, AL10.AL_BUFFERS_QUEUED, out nbQueued);
            for (int i = 0; i < nbQueued; ++i)
                AL10.alSourceUnqueueBuffers(_source, (IntPtr)1, ref buffer);
            AL10.alSourcei(_source, AL10.AL_BUFFER, 0);

            // 소스를 삭제합니다
            AL10.alDeleteSources((IntPtr)1, ref _source);

            // 퍼버들을 삭제합니다
            AL10.alDeleteBuffers((IntPtr)_numBuffers, _buffers);

            switch (_format)
            {
                case MusicFormat.It:
                    _itDecoder.Unload();
                    break;

                case MusicFormat.Ogg:
                    Vorbisfile.ov_clear(ref _oggFile);
                    _oggMemHandle.Free();
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
                    case MusicFormat.It:
                        DecodeIt(buffer, 4096);
                        break;

                    case MusicFormat.Ogg:
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

        void DecodeIt(uint destinationBuffer, int nbSamples)
        {
            byte[] rawData = new byte[nbSamples * 2];
            int bytesRead = _itDecoder.Decode(rawData, nbSamples);

            if (bytesRead > 0)
            {
                AL10.alBufferData(destinationBuffer, AL10.AL_FORMAT_STEREO16, rawData, (IntPtr)bytesRead, (IntPtr)44100);

                int error = AL10.alGetError();
                if (error != AL10.AL_NO_ERROR)
                    Debug.Die("Failed to fill the audio buffer with decoded IT data for music file '{0}': error {1}"
                        .F(_fileName, error));
            }
        }
        #endregion
    }
}
