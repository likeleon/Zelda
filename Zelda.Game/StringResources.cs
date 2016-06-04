using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Zelda.Game
{
    [XmlRoot("Strings")]
    public class StringResources : IXmlDeserialized, IPrepareXmlSerialize
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

        [XmlIgnore]
        public IReadOnlyDictionary<string, string> Strings => _strings;

        readonly SortedDictionary<string, string> _strings = new SortedDictionary<string, string>(StringComparer.Ordinal);

        public void OnDeserialized()
        {
            Texts.EmptyIfNull().Do(t => _strings.Add(t.Key, t.Value));
        }

        public void OnPrepareSerialize()
        {
            Texts = _strings.Select(x => new Text() { Key = x.Key, Value = x.Value }).ToArray();
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
            if (!HasString(key))
                throw new ArgumentException("No such string: '{0}'".F(key), nameof(key));

            return _strings[key];
        }

        public void SetString(string key, string value)
        {
            if (!HasString(key))
                throw new ArgumentException("No such string: '{0}'".F(key), nameof(key));

            _strings[key] = value;
        }

        public void AddString(string key, string str)
        {
            if (HasString(key))
                throw new ArgumentException("Duplicate string '{0}'".F(key), nameof(key));

            _strings.Add(key, str);
        }

        public bool RemoveString(string key)
        {
            return _strings.Remove(key);
        }

        public void SetStringKey(string oldKey, string newKey)
        {
            if (!HasString(oldKey))
                throw new ArgumentException("No such string: '{0}'".F(oldKey), nameof(oldKey));

            if (HasString(newKey))
                throw new ArgumentException("Duplicate string '{0}'".F(newKey), nameof(newKey));

            var str = GetString(oldKey);
            RemoveString(oldKey);
            AddString(newKey, str);
        }
    }
}
