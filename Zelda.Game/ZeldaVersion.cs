using System;
using System.Reflection;

namespace Zelda.Game
{
    public static class ZeldaVersion
    {
        static readonly Version _version = Assembly.GetExecutingAssembly().GetName().Version;

        public static Version Version {  get { return _version; } }
    }
}
