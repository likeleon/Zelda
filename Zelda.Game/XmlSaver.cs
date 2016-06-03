using System.IO;

namespace Zelda.Game
{
    public static class XmlSaver
    {
        public static void Save(object o, string fileName)
        {
            var tmpFileName = fileName + ".zelda_tmp";
            try
            {
                using (var outFile = File.OpenWrite(tmpFileName))
                {
                    (o as IPrepareXmlSerialize)?.OnPrepareSerialize();
                    o.XmlSerialize(outFile);
                }

                if (File.Exists(fileName))
                    File.Delete(fileName);
                File.Move(tmpFileName, fileName);
            }
            finally
            {
                File.Delete(tmpFileName);
            }
        }
    }
}
