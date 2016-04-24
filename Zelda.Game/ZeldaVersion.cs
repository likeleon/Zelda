using System;
using System.Reflection;

namespace Zelda.Game
{
    public static class ZeldaVersion
    {
        public static Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;
    }
}
