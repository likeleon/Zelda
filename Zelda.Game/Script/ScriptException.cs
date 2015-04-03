using System;

namespace Zelda.Game.Script
{
    class ScriptException : Exception
    {
        public ScriptException(string s)
            : base(s)
        {
        }
    }
}
