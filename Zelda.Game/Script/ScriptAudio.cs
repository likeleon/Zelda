using System;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Script
{
    public class ScriptAudio
    {
        public static int MusicVolume { get { return Music.Volume; } }
        public static int SoundVolume { get { return Audio.Volume; } }

        public static void PlaySound(string soundId)
        {
            ScriptToCore.Call(() =>
            {
                if (!Audio.Exists(soundId))
                    throw new ArgumentException("No such sound: '{0}'".F(soundId), "soundId");
                Audio.Play(soundId);
            });
        }

        public static void PreloadSounds()
        {
            ScriptToCore.Call(Audio.LoadAll);
        }

        public static void PlayMusic(string musicId, bool loop = true)
        {
            ScriptToCore.Call(() => PlayMusic(musicId, loop, null));
        }

        public static void PlayMusic(string musicId, Action callback)
        {
            ScriptToCore.Call(() => PlayMusic(musicId, false, callback));
        }

        static void PlayMusic(string musicId, bool loop, Action callback)
        {
            if (String.IsNullOrEmpty(musicId))
                Music.StopPlaying();
            else
            {
                if (!Music.Exists(musicId))
                    throw new ArgumentException("No such music: '{0}'".F(musicId), "musicId");

                Music.Play(musicId, loop, callback);
            }
        }

        public static void StopMusic()
        {
            ScriptToCore.Call(Music.StopPlaying);
        }

        public static void SetMusicVolume(int volume)
        {
            ScriptToCore.Call(() => Music.SetVolume(volume));
        }

        public static void SetSoundVolume(int volume)
        {
            ScriptToCore.Call(() => Audio.SetVolume(volume));
        }
    }
}
