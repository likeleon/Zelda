using OpenAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Zelda.Game.LowLevel
{
    [StructLayout(LayoutKind.Sequential)]
    class SoundFromMemory
    {
        public byte[] data;
        public uint position;
        public bool loop;
    }

    class Sound : IDisposable
    {
        readonly string _id;
        readonly Queue<uint> _sources = new Queue<uint>();
        uint _buffer = AL10.AL_NONE;

        public Sound(string id = "")
        {
            _id = id;
        }

        public void Dispose()
        {
            foreach (uint source in _sources)
            {
                AL10.alSourceStop(source);
                AL10.alSourcei(source, AL10.AL_BUFFER, 0);
                uint sourceToDelete = source;
                AL10.alDeleteSources((IntPtr)1, ref sourceToDelete);
            }

            if (_buffer != AL10.AL_NONE)
                AL10.alDeleteBuffers((IntPtr)1, ref _buffer);
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

        public bool Start(float volume)
        {
            bool success = false;

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
            AL10.alSourcef(source, AL10.AL_GAIN, volume);

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
                AL10.alSourcePlay(source);
                error = AL10.alGetError();
                if (error != AL10.AL_NO_ERROR)
                    Debug.Error("Cannot play sound '{0}': error {1}".F(_id, error));
                else
                    success = true;
            }
            return success;
        }

        public bool UpdatePlaying()
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

            if (!Core.Mod.ModFiles.DataFileExists(fileName))
                throw new Exception("Cannot find sound file '{0}'".F(fileName));

            // 사운드 파일을 읽어들입니다
            var mem = new SoundFromMemory();
            mem.loop = false;
            mem.position = 0;
            mem.data = Core.Mod.ModFiles.DataFileRead(fileName);
            var memHandle = GCHandle.Alloc(mem);

            var file = IntPtr.Zero;
            try
            {
                int error = Vorbisfile.ov_open_callback(GCHandle.ToIntPtr(memHandle), out file, IntPtr.Zero, 0, Audio.OggCallbacks);
                if (error != 0)
                    throw new Exception("Cannot load sound file '{0}' from memory: error {1}".F(fileName, error));

                // 인코딩된 사운드의 속성을 읽습니다
                var info = Vorbisfile.ov_info(file, -1);
                int sampleRate = info.rate;

                int format;
                if (info.channels == 1)
                    format = AL10.AL_FORMAT_MONO16;
                else if (info.channels == 2)
                    format = AL10.AL_FORMAT_STEREO16;
                else
                    throw new Exception("Invalid audio format for sound file '{0}'".F(fileName));

                // vorbisfile로 디코딩
                var samples = new MemoryStream();
                int bitstream;
                int bytesRead;
                int totalBytesRead = 0;
                int bufferSize = 4096;
                byte[] samplesBuffer = new byte[bufferSize];
                do
                {
                    bytesRead = Vorbisfile.ov_read(file, samplesBuffer, bufferSize, 0, 2, 1, out bitstream);
                    if (bytesRead < 0)
                        throw new Exception("Error while decoding ogg chunk in sound file '{0}': {1}".F(fileName, bytesRead));

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
                while (bytesRead > 0);

                // 샘플을 OpenAL 퍼버로 복사합니다
                AL10.alGenBuffers((IntPtr)1, out buffer);
                if (AL10.alGetError() != AL10.AL_NO_ERROR)
                    throw new Exception("Failed to generate audio buffer");

                AL10.alBufferData(buffer, AL10.AL_FORMAT_STEREO16, samples.GetBuffer(), (IntPtr)totalBytesRead, (IntPtr)sampleRate);
                error = AL10.alGetError();
                if (error != AL10.AL_NO_ERROR)
                    throw new Exception("Cannot copy the sound samples of '{0}' into buffer {1}: error {2}".F(fileName, buffer, error));

                return buffer;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (file != IntPtr.Zero)
                    Vorbisfile.ov_clear(ref file);

                memHandle.Free();
            }
        }
    }
}
