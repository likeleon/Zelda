using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Zelda.Game
{
    public class StringResources : XmlData
    {
        readonly SortedDictionary<string, string> _strings = new SortedDictionary<string, string>(StringComparer.Ordinal);

        public IReadOnlyDictionary<string, string> Strings { get { return _strings; } }

        protected override bool OnImportFromBuffer(byte[] buffer)
        {
            try
            {
                var data = buffer.XmlDeserialize<StringXmlData>();
                data.Texts.EmptyIfNull().Do(d => _strings.Add(d.Key, d.Value));
                return true;
            }
            catch (Exception ex)
            {
                Debug.Error("Failed to import string resources: {0}".F(ex));
                return false;
            }
        }

        protected override bool OnExportToStream(Stream stream)
        {
            try
            {
                var data = new StringXmlData();
                data.Texts = _strings.Select(kvp => new StringXmlData.Text()
                {
                    Key = kvp.Key,
                    Value = kvp.Value
                }).ToArray();
                data.XmlSerialize(stream);
                return true;
            }
            catch (Exception ex)
            {
                Debug.Error("Failed to export string resources: {0}".F(ex));
                return false;
            }
        }

        public void Clear()
        {
            _strings.Clear();
        }

        public bool HasString(string key)
        {
            return _strings.ContainsKey(key);
        }

        public string GetString(string key)
        {
            Debug.CheckAssertion(HasString(key), "No such string: '{0}'".F(key));
            return _strings[key];
        }

        public void SetString(string key, string value)
        {
            Debug.CheckAssertion(HasString(key), "No such string: '{0}'".F(key));
            _strings[key] = value;
        }

        public bool AddString(string key, string @string)
        {
            if (_strings.ContainsKey(key))
                return false;
            _strings.Add(key, @string);
            return true;
        }

        public bool RemoveString(string key)
        {
            return _strings.Remove(key);
        }

        public bool SetStringKey(string oldKey, string newKey)
        {
            if (!HasString(oldKey))
                return false;

            if (HasString(newKey))
                return false;

            var @string = GetString(oldKey);
            RemoveString(oldKey);
            AddString(newKey, @string);
            return true;
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
