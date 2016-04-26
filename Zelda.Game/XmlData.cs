using System.IO;
using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    // XML로부터 로드되거나 저장(이건 옵션)될 수 있는 추상 클래스
    public abstract class XmlData
    {
        public bool ImportFromFile(string fileName)
        {
            if (!File.Exists(fileName))
                Debug.Error("Failed to load data file '{0}'".F(fileName));

            return OnImportFromBuffer(File.ReadAllBytes(fileName));
        }

        public bool ImportFromModFile(string modFileName, bool languageSpecific = false)
        {
            if (!ModFiles.DataFileExists(modFileName))
                Debug.Error("Cannot find mod file '{0}'".F(modFileName));

            return OnImportFromBuffer(ModFiles.DataFileRead(modFileName, languageSpecific));
        }

        public bool ImportFromBuffer(byte[] buffer)
        {
            return OnImportFromBuffer(buffer);
        }

        public bool ExportToFile(string fileName)
        {
            var tmpFileName = fileName + ".zelda_tmp";

            using (var outFile = File.OpenWrite(tmpFileName))
            {
                if (!OnExportToStream(outFile))
                {
                    outFile.Close();
                    File.Delete(tmpFileName);
                    return false;
                }
            }

            if (File.Exists(fileName))
                File.Delete(fileName);
            File.Move(tmpFileName, fileName);

            return true;
        }

        protected abstract bool OnImportFromBuffer(byte[] buffer);

        protected virtual bool OnExportToStream(Stream stream)
        {
            return false;
        }
    }
}
