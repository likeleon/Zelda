using System.IO;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    // XML로부터 로드되거나 저장(이건 옵션)될 수 있는 추상 클래스
    public abstract class XmlData
    {
        public bool ImportFromModFile(string modFileName)
        {
            if (!ModFiles.DataFileExists(modFileName))
                throw new InvalidDataException("Cannot find mod file '" + modFileName + "'");

            using (Stream stream = ModFiles.DataFileRead(modFileName))
            {
                return ImportFromStream(stream);
            }
        }

        protected abstract bool ImportFromStream(Stream stream);
    }
}
