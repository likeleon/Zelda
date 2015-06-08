using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    static class StringResource
    {
        static readonly Dictionary<string, string> _strings = new Dictionary<string, string>();

        public static void Initialize()
        {
            _strings.Clear();

            var fileName = "text/strings.xml";
            try
            {
                var stream = ModFiles.DataFileRead(fileName, true);
                var data = stream.XmlDeserialize<StringXmlData>();
                foreach (var text in data.Texts.EmptyIfNull())
                    _strings.Add(text.Key, text.Value);
            }
            catch (Exception ex)
            {
                Debug.Error("Failed to load strings file '{0}' for language '{1}': {2}"
                    .F(fileName, Language.LanguageCode, ex.ToString()));
            }
        }

        public static void Quit()
        {
            _strings.Clear();
        }

        public static bool Exists(string key)
        {
            return _strings.ContainsKey(key);
        }

        public static string GetString(string key)
        {
            Debug.CheckAssertion(Exists(key), "Cannot find string with key '{0}'".F(key));
            return _strings[key];
        }
    }

    [XmlRoot("Strings")]
    public class StringXmlData
    {
        public class Text
        {
            [XmlAttribute]
            public string Key { get; set; }

            [XmlText]
            public string Value { get; set; }
        }

        [XmlElement("Text")]
        public Text[] Texts { get; set; }
    }
}
