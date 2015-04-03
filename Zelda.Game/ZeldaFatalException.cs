using System;

namespace Zelda.Game
{
	class ZeldaFatalException : Exception
    {
        public ZeldaFatalException(string s)
            : base(s)
        {
        }
    }
}
