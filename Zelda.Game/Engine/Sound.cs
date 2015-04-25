using OpenAL;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Zelda.Game.Engine
{
    class Sound
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
    }
}
