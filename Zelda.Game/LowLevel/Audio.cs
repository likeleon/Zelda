using OpenAL;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Zelda.Game.LowLevel
{
    public class Audio : IDisposable
    {
        public int SoundVolume
        {
            get { return (int)(_volume * 100.0 + 0.5); }
            set { _volume = Math.Min(100, Math.Max(0, value)) / 100.0f; }
        }

        public int MusicVolume
        {
            get { return Music.Volume; }
            set { Music.SetVolume(value); }
        }

        internal static readonly Vorbisfile.ov_callbacks OggCallbacks = CreateCallbacks();

        readonly List<Sound> _currentSounds = new List<Sound>();
        readonly Dictionary<string, Sound> _allSounds = new Dictionary<string, Sound>();
        IntPtr _device;
        IntPtr _context;        
        float _volume = 1.0f;
        bool _soundsPreloaded;

        static Vorbisfile.ov_callbacks CreateCallbacks()
        {
            var cb = new Vorbisfile.ov_callbacks();
            cb.read_func = CbRead;
            cb.seek_func = null;
            cb.close_func = null;
            cb.tell_func = null;
            return cb;
        }

        internal Audio(Arguments args)
        {
            if (args.HasArgument("-no-audio"))
                return;

            // OpenAL 초기화
            _device = ALC10.alcOpenDevice(null);
            if (_device == IntPtr.Zero)
                throw new Exception("Cannot open audio device");

            int[] attr = { ALC10.ALC_FREQUENCY, 32000, 0 }; // SPC 출력 샘플링 레이트가 32 KHZ
            _context = ALC10.alcCreateContext(_device, attr);
            if (_context == IntPtr.Zero)
                throw new Exception("Cannot create audio context");

            if (!ALC10.alcMakeContextCurrent(_context))
                throw new Exception("Cannot activate audio context");

            AL10.alGenBuffers(IntPtr.Zero, null);   // 몇몇 시스템에서 첫 사운드가 로드될 때 발생하는 에러 대비

            SoundVolume = 100;

            Music.Initialize();
        }

        public void Dispose()
        {
            Music.Quit();

            _allSounds.Values.Do(s => s.Dispose());

            if (_context != IntPtr.Zero)
            {
                ALC10.alcMakeContextCurrent(IntPtr.Zero);
                ALC10.alcDestroyContext(_context);
            }

            if (_device != IntPtr.Zero)
                ALC10.alcCloseDevice(_device);
        }

        internal void Update()
        {
            var soundsToRemove = new List<Sound>();
            foreach (var sound in _currentSounds)
            {
                if (!sound.UpdatePlaying())
                    soundsToRemove.Add(sound);
            }

            soundsToRemove.Do(s => _currentSounds.Remove(s));

            Music.Update();
        }

        static uint CbRead(IntPtr ptr, uint size, uint nbBytes, IntPtr datasource)
        {
            var handle = GCHandle.FromIntPtr(datasource);
            var mem = (SoundFromMemory)handle.Target;

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

            Marshal.Copy(mem.data, (int)mem.position, ptr, (int)nbBytes);
            mem.position += nbBytes;

            return nbBytes;
        }

        public void PreloadSounds() => LoadAll();

        internal void LoadAll()
        {
            if (_soundsPreloaded)
                return;

            var soundElements = Core.Mod.GetResources(ResourceType.Sound);
            foreach (var soundId in soundElements.Keys)
            {
                _allSounds[soundId] = new Sound(soundId);
                _allSounds[soundId].Load();
            }

            _soundsPreloaded = true;
        }

        internal bool Exists(string soundId)
        {
            return Core.Mod.ModFiles.DataFileExists("sounds/{0}.ogg".F(soundId));
        }

        public void PlaySound(string soundId)
        {
            if (!Exists(soundId))
                throw new Exception("No such sound: '{0}'".F(soundId));

            Play(soundId);
        }

        internal void Play(string soundId)
        {
            if (!_allSounds.ContainsKey(soundId))
                _allSounds.Add(soundId, new Sound(soundId));

            if (!_allSounds[soundId].Start(_volume))
                return;

            _currentSounds.Remove(_allSounds[soundId]);    // 중복을 막기 위해
            _currentSounds.Add(_allSounds[soundId]);
        }

        public void PlayMusic(string musicId, bool loop = true) => PlayMusic(musicId, loop, null);
        public void PlayMusic(string musicId, Action callback) => PlayMusic(musicId, false, callback);

        void PlayMusic(string musicId, bool loop, Action callback)
        {
            if (!Music.Exists(musicId))
                throw new ArgumentException("No such music: '{0}'".F(musicId), "musicId");

            Music.Play(musicId, loop, callback);
        }

        public void StopMusic() => Music.StopPlaying();
    }
}
