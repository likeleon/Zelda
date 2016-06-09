using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Zelda.Game
{
    public class DialogData : IXmlDeserialized, IPrepareXmlSerialize
    {
        [XmlAttribute]
        public string Id { get; set; }

        [XmlElement]
        public string Text { get; set; }

        [XmlAnyElement]
        public XmlElement[] PropertyElements { get; set; }

        [XmlIgnore]
        public IReadOnlyDictionary<string, string> Properties => _properties;

        readonly SortedDictionary<string, string> _properties = new SortedDictionary<string, string>(StringComparer.Ordinal);

        public void OnDeserialized()
        {
            PropertyElements.EmptyIfNull().Do(e => _properties.Add(e.Name, e.InnerText));
        }

        public void OnPrepareSerialize()
        {
            var doc = new XmlDocument();
            var elements = new List<XmlElement>();
            foreach (var kvp in _properties)
            {
                var element = doc.CreateElement(kvp.Key);
                element.InnerText = kvp.Value;
                elements.Add(element);
            }
            PropertyElements = elements.ToArray();
        }

        public bool HasProperty(string key)
        {
            if (key == "Text" || key == "DialogId")
                return false;

            return _properties.ContainsKey(key);
        }

        public string GetProperty(string key)
        {
            if (!HasProperty(key))
                throw new ArgumentException("No such dialog property : '{0}'".F(key), nameof(key));

            return _properties[key];
        }

        public void SetProperty(string key, string value)
        {
            if (key == "Text" || key == "DialogId")
                return;

            _properties[key] = value;
        }

        public bool RemoveProperty(string key)
        {
            return _properties.Remove(key);
        }
    }

    [XmlRoot("Dialogs")]
    public class DialogResources : IXmlDeserialized, IPrepareXmlSerialize
    {
        [XmlElement("Dialog")]
        public DialogData[] DialogDatas { get; set; }

        [XmlIgnore]
        public IReadOnlyDictionary<string, DialogData> Dialogs => _dialogs;

        Dictionary<string, DialogData> _dialogs;

        public void OnDeserialized()
        {
            _dialogs = DialogDatas.EmptyIfNull().ToDictionary(d => d.Id, d => d);
        }

        public void OnPrepareSerialize()
        {
            DialogDatas.Do(d => d.OnPrepareSerialize());
        }

        public void Clear()
        {
            _dialogs.Clear();
        }

        public bool HasDialog(string dialogId)
        {
            return _dialogs.ContainsKey(dialogId);
        }

        public DialogData GetDialog(string dialogId)
        {
            if (!HasDialog(dialogId))
                throw new ArgumentException("No such dialog: '{0}'".F(dialogId), nameof(dialogId));

            return _dialogs[dialogId];
        }

        public void AddDialog(string dialogId, DialogData dialog)
        {
            if (HasDialog(dialogId))
                throw new ArgumentException("Duplicate dialog id '{0}'".F(dialogId), nameof(dialogId));

            _dialogs.Add(dialogId, dialog);
        }

        public bool RemoveDialog(string dialogId)
        {
            return _dialogs.Remove(dialogId);
        }

        public void SetDialogId(string oldDialogId, string newDialogId)
        {
            if (!HasDialog(oldDialogId))
                throw new ArgumentException("No such dialog '{0}'".F(oldDialogId), nameof(oldDialogId));

            if (HasDialog(newDialogId))
                throw new ArgumentException("Duplicate dialog id '{0}'".F(newDialogId), nameof(newDialogId));

            var dialog = GetDialog(oldDialogId);
            RemoveDialog(oldDialogId);
            dialog.Id = newDialogId;
            AddDialog(newDialogId, dialog);
        }
    }
}
