using System;
using System.Reflection;

namespace Zelda.Game
{
    public static class ZeldaVersion
    {
        public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;
    }
}
