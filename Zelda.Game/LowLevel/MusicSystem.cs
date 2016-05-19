using OpenAL;
using System;

namespace Zelda.Game.LowLevel
{
    class MusicSystem : IDisposable
    {
        readonly ItDecoder _itDecoder = new ItDecoder();
        float _volume = 1.0f;
        Music _currentMusic;

        public string CurrentMusicId => _currentMusic?.Id ?? Music.None;
        public MusicFormat Format => _currentMusic?.Format ?? MusicFormat.NoFormat;

        public int Volume
        {
            get { return (int)(_volume * 100.0 + 0.5); }
            set
            {
                _volume = Math.Min(100, Math.Max(0, value)) / 100.0f;

                if (_currentMusic != null)
                    AL10.alSourcef(_currentMusic.Source, AL10.AL_GAIN, _volume);
            }
        }

        public MusicSystem()
        {
            Volume = 100;
        }

        public void Dispose()
        {
            _itDecoder.Dispose();
        }

        public void Update()
        {
            if (_currentMusic == null)
                return;

            bool playing = _currentMusic.UpdatePlaying();
            if (playing)
                return;

            var callback = _currentMusic.Callback;
            _currentMusic = null;
            callback?.Invoke();
        }

        public bool Exists(string musicId)
        {
            if (musicId == Music.None || musicId == Music.Unchanged)
                return true;

            MusicFormat format;
            return FindMusicFile(musicId, out format) != null;
        }

        public void Play(string musicId, bool loop, Action callback = null)
        {
            if (musicId == Music.Unchanged || musicId == CurrentMusicId)
                return;

            if (_currentMusic != null)
            {
                _currentMusic.Stop();
                _currentMusic = null;
            }

            if (musicId != Music.None)
            {
                _currentMusic = new Music(_itDecoder, musicId, loop, callback);
                _currentMusic.Start(Volume);
            }
        }

        public void StopPlaying()
        {
            Play(Music.None, false);
        }

        static internal string FindMusicFile(string musicId, out MusicFormat format)
        {
            format = MusicFormat.Ogg;

            string fileNameStart = "musics/" + musicId;
            if (Core.Mod.ModFiles.DataFileExists(fileNameStart + ".ogg"))
            {
                format = MusicFormat.Ogg;
                return fileNameStart + ".ogg";
            }
            else if (Core.Mod.ModFiles.DataFileExists(fileNameStart + ".it"))
            {
                format = MusicFormat.It;
                return fileNameStart + ".it";
            }
            else
                return null;
        }

        public int GetNumChannels()
        {
            Debug.CheckAssertion(Format == MusicFormat.It,
                "This function is only supported for .it musics");

            return _itDecoder.GetNumChannels();
        }

        public int GetChannelVolume(int channel)
        {
            Debug.CheckAssertion(Format == MusicFormat.It,
                "This function is only supported for .it musics");

            return _itDecoder.GetChannelVolume(channel);
        }

        public void SetChannelVolume(int channel, int volume)
        {
            Debug.CheckAssertion(Format == MusicFormat.It,
                "This function is only supported for .it musics");

            _itDecoder.SetChannelVolume(channel, volume);
        }
    }
}
