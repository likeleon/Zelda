using System;
using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    public class ScriptAudio
    {
        public static void PlaySound(string soundId)
        {
            ScriptToCore.Call(() =>
            {
                if (!Sound.Exists(soundId))
                    throw new ArgumentException("No such sound: '{0}'".F(soundId), "soundId");
                Sound.Play(soundId);
            });
        }
    }
}
