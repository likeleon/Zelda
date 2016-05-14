using Zelda.Game.LowLevel;

namespace Zelda.Game.Script
{
    public class ScriptFile
    {
        public static bool Exists(string fileName)
        {
            return ScriptToCore.Call(() => MainLoop.ModFiles.DataFileExists(fileName, false));
        }
    }
}
