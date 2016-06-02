using System;
using System.IO;
using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    public interface IXmlDeserialized
    {
        void OnDeserialized();
    }

    public interface IXmlSerializing
    {
        void OnSerializing();
    }

    public static class XmlLoader
    {
        public static T Load<T>(string fileName)
        {
            if (!File.Exists(fileName))
                throw new Exception("No such file '{0}'".F(fileName));

            return Load<T>(File.ReadAllBytes(fileName));
        }

        public static T Load<T>(byte[] buffer)
        {
            var obj = buffer.XmlDeserialize<T>();
            (obj as IXmlDeserialized)?.OnDeserialized();
            return obj;
        }

        public static T Load<T>(ModFiles modFiles, string fileName, bool languageSpecific = false)
        {
            if (!modFiles.DataFileExists(fileName))
                throw new Exception("Cannot find mod file '{0}".F(fileName));

            return Load<T>(modFiles.DataFileRead(fileName, languageSpecific));
        }
    }

}
