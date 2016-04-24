using System;
using Zelda.Game.Lowlevel;

namespace Zelda.Game.Script
{
    public class ScriptAudio
    {
        public static int MusicVolume { get { return Music.Volume; } }
        public static int SoundVolume { get { return Sound.Volume; } }

        public static void PlaySound(string soundId)
        {
            ScriptToCore.Call(() =>
            {
                if (!Sound.Exists(soundId))
                    throw new ArgumentException("No such sound: '{0}'".F(soundId), "soundId");
                Sound.Play(soundId);
            });
        }

        public static void PreloadSounds()
        {
            ScriptToCore.Call(Sound.LoadAll);
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
            ScriptToCore.Call(() => Sound.SetVolume(volume));
        }
    }
}
