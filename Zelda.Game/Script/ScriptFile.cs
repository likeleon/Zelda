using Zelda.Game.Lowlevel;

namespace Zelda.Game.Script
{
    public class ScriptFile
    {
        public static bool Exists(string fileName)
        {
            return ScriptToCore.Call(() => ModFiles.DataFileExists(fileName, false));
        }
    }
}
