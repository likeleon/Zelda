using System.IO;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    // XML로부터 로드되거나 저장(이건 옵션)될 수 있는 추상 클래스
    public abstract class XmlData
    {
        public bool ImportFromFile(string fileName)
        {
            if (!File.Exists(fileName))
                Debug.Error("Failed to load data file '{0}'".F(fileName));

            return ImportFromBuffer(File.ReadAllBytes(fileName));
        }

        public bool ImportFromModFile(string modFileName)
        {
            if (!ModFiles.DataFileExists(modFileName))
                Debug.Error("Cannot find mod file '{0}'".F(modFileName));

            return ImportFromBuffer(ModFiles.DataFileRead(modFileName));
        }

        public bool ExportToFile(string fileName)
        {
            var tmpFileName = fileName + ".zelda_tmp";

            using (var outFile = File.OpenWrite(tmpFileName))
            {
                if (!ExportToStream(outFile))
                {
                    outFile.Close();
                    File.Delete(tmpFileName);
                    return false;
                }
            }

            File.Copy(tmpFileName, fileName, true);
            return true;
        }

        protected abstract bool ImportFromBuffer(byte[] buffer);

        protected virtual bool ExportToStream(Stream stream)
        {
            return false;
        }
    }
}
