using OpenAL;
using System;
using System.Linq;
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
        public const string None = "None";
        public const string Unchanged = "Same";

        public string Id { get; }
        public bool Loop { get; }
        public Action Callback { get; private set; }
        public MusicFormat Format => _format;
        public uint Source => _source;

        readonly ItDecoder _itDecoder;
        readonly uint[] _buffers;

        MusicFormat _format = MusicFormat.Ogg;
        uint _source = AL10.AL_NONE;
        string _fileName;

        // OGG 한정
        IntPtr _oggFile;
        readonly SoundFromMemory _oggMem = new SoundFromMemory();
        GCHandle _oggMemHandle;

        public Music(ItDecoder itDecoder, string musicId, bool loop, Action callback)
        {
            if (loop && callback != null)
                throw new Exception("Attemp to set both a loop and a callback to music");

            _itDecoder = itDecoder;
            Id = musicId;
            Loop = loop;
            Callback = callback;

            _buffers = Enumerable.Repeat<uint>(AL10.AL_NONE, 8).ToArray();
        }

        public void Start(int volume)
        {
            if (_fileName == null)
            {
                _fileName = MusicSystem.FindMusicFile(Id, out _format);
                if (_fileName == null)
                    throw new Exception("Cannot find music file 'musics/{0}' (tried with extentions .ogg)".F(Id));
            }

            // 버퍼와 소스를 만듭니다
            AL10.alGenBuffers((IntPtr)_buffers.Length, _buffers);
            AL10.alGenSources((IntPtr)1, out _source);
            AL10.alSourcef(_source, AL10.AL_GAIN, volume);

            // 음악을 메모리로 읽어옵니다
            byte[] soundBuffer;

            switch (_format)
            {
                case MusicFormat.It:
                    soundBuffer = Core.Mod.ModFiles.DataFileRead(_fileName);
                    _itDecoder.Load(soundBuffer);
                    _buffers.Do(b => DecodeIt(b, 4096));
                    break;

                case MusicFormat.Ogg:
                    _oggMem.position = 0;
                    _oggMem.loop = Loop;
                    _oggMem.data = Core.Mod.ModFiles.DataFileRead(_fileName);
                    _oggMemHandle = GCHandle.Alloc(_oggMem);
                    // 이제 _oggmem은 인코딩된 데이터를 가집니다

                    int error = Vorbisfile.ov_open_callback(GCHandle.ToIntPtr(_oggMemHandle), out _oggFile, IntPtr.Zero, 0, Audio.OggCallbacks);
                    if (error != 0)
                        throw new Exception("Cannot load music file '{0}' from memory: error {1}".F(_fileName, error));

                    _buffers.Do(b => DecodeOgg(b, 4096));
                    break;

                default:
                    throw new Exception("Invalid music format");
            }

            // 스트리밍을 시작합니다
            AL10.alSourceQueueBuffers(_source, (IntPtr)_buffers.Length, _buffers);
            int alError = AL10.alGetError();
            if (alError != AL10.AL_NO_ERROR)
                throw new Exception("Cannot initialize buffers for music '{0}': error {1}".F(_fileName, alError));

            AL10.alSourcePlay(_source);
        }

        public void Stop()
        {
            Callback = null;

            AL10.alSourceStop(_source);

            int nbQueued;
            uint buffer = 0;
            AL10.alGetSourcei(_source, AL10.AL_BUFFERS_QUEUED, out nbQueued);
            nbQueued.Times(() => AL10.alSourceUnqueueBuffers(_source, (IntPtr)1, ref buffer));
            AL10.alSourcei(_source, AL10.AL_BUFFER, 0);

            // 소스를 삭제합니다
            AL10.alDeleteSources((IntPtr)1, ref _source);

            // 퍼버들을 삭제합니다
            AL10.alDeleteBuffers((IntPtr)_buffers.Length, _buffers);

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

        public bool UpdatePlaying()
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
                        throw new Exception("Invalid music format");
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
            var info = Vorbisfile.ov_info(_oggFile, -1);
            int sampleRate = info.rate;

            int alFormat = AL10.AL_NONE;
            if (info.channels == 1)
                alFormat = AL10.AL_FORMAT_MONO16;
            else if (info.channels == 2)
                alFormat = AL10.AL_FORMAT_STEREO16;

            // OGG 데이터 디코딩
            var rawData = new byte[nbSamples * info.channels];
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
                        throw new Exception("Error while decoding ogg chunk: {0}".F(bytesRead));
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
                throw new Exception("Failed to fill the audio buffer with decoded OGG data for music file '{0}': error {1}".F(_fileName, error));
        }

        void DecodeIt(uint destinationBuffer, int nbSamples)
        {
            var rawData = new byte[nbSamples * 2];
            var bytesRead = _itDecoder.Decode(rawData, nbSamples);

            if (bytesRead <= 0)
                return;

            AL10.alBufferData(destinationBuffer, AL10.AL_FORMAT_STEREO16, rawData, (IntPtr)bytesRead, (IntPtr)44100);

            int error = AL10.alGetError();
            if (error != AL10.AL_NO_ERROR)
                throw new Exception("Failed to fill the audio buffer with decoded IT data for music file '{0}': error {1}".F(_fileName, error));
        }
    }
}
