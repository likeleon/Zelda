using System;
using Zelda.Game.Script;

namespace Alttp
{
    public class Main : ScriptMain
    {
        bool _debugEnabled;

        protected override void OnStarted()
        {
            LoadSettings();

            _debugEnabled = ScriptFile.Exists("debug");
        }
    }
}
