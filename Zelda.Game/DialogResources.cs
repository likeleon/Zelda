using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Zelda.Game
{
    [XmlRoot("Dialogs")]
    public class DialogXmlData
    {
        public class Dialog
        {
            [XmlAttribute]
            public string Id { get; set; }
          
            [XmlElement]
            public string Text { get; set; }

            [XmlAnyElement]
            public XmlElement[] PropertyElements { get; set; }
        }

        [XmlElement("Dialog")]
        public Dialog[] Dialogs { get; set; }
    }

    class DialogData
    {
        readonly Dictionary<string, string> _properties = new Dictionary<string, string>();

        public string Id { get; set; }
        public string Text { get; set; }
        public IReadOnlyDictionary<string, string> Properties { get { return _properties; } }

        public bool HasProperty(string key)
        {
            if (key.IsNullOrEmpty() || key == "Text" || key == "DialogId")
                return false;

            return _properties.ContainsKey(key);
        }

        public string GetProperty(string key)
        {
            Debug.CheckAssertion(HasProperty(key), "No such dialog property : '{0}'".F(key));
            return _properties[key];
        }

        public void SetProperty(string key, string value)
        {
            if (key.IsNullOrEmpty() || key == "Text" || key == "DialogId")
                return;

            _properties[key] = value;
        }

        public bool RemoveProperty(string key)
        {
            return _properties.Remove(key);
        }
    }

    class DialogResources : XmlData
    {
        Dictionary<string, DialogData> _dialogs = new Dictionary<string, DialogData>();

        public IReadOnlyDictionary<string, DialogData> Dialogs { get { return _dialogs; } }

        protected override bool OnImportFromBuffer(byte[] buffer)
        {
            try
            {
                var data = buffer.XmlDeserialize<DialogXmlData>();
                foreach (var dialogData in data.Dialogs.EmptyIfNull())
                {
                    DialogData dialog = new DialogData()
                    {
                        Id = dialogData.Id,
                        Text = dialogData.Text
                    };

                    foreach (var element in dialogData.PropertyElements.EmptyIfNull())
                        dialog.SetProperty(element.Name, element.InnerText);

                    if (dialog.Id.IsNullOrEmpty())
                        throw new InvalidDataException("Missing value dialog Id");

                    if (dialog.Text.IsNullOrEmpty())
                        throw new InvalidDataException("Missing text for dialog '{0}'".F(dialog.Id));

                    if (HasDialog(dialog.Id))
                        throw new InvalidDataException("Duplicate dialog '{0}'".F(dialog.Id));

                    _dialogs.Add(dialog.Id, dialog);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.Error("Failed to import dialog resources: {0}".F(ex));
                return false;
            }
        }

        protected override bool OnExportToStream(Stream stream)
        {
            try
            {
                var data = new DialogXmlData();
                var dialogs = new List<DialogXmlData.Dialog>();
                foreach (var dataDialog in _dialogs.Values)
                {
                    dialogs.Add(new DialogXmlData.Dialog()
                    {
                        Id = dataDialog.Id,
                        Text = dataDialog.Text,
                        PropertyElements = ExportProperties(dataDialog)
                    });
                }
                data.Dialogs = dialogs.ToArray();
                return true;
            }
            catch (Exception ex)
            {
                Debug.Error("Failed to export dialog resources: {0}".F(ex));
                return false;
            }
        }

        XmlElement[] ExportProperties(DialogData dialog)
        {
            var doc = new XmlDocument();
            var elements = new List<XmlElement>();
            foreach (var property in dialog.Properties)
            {
                var element = doc.CreateElement(property.Key);
                element.InnerText = property.Value;
                elements.Add(element);
            }
            return elements.ToArray();
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
            Debug.CheckAssertion(HasDialog(dialogId), "No such dialog: '{0}'".F(dialogId));
            return _dialogs[dialogId];
        }

        public bool AddDialog(string dialogId, DialogData dialog)
        {
            if (HasDialog(dialogId))
                return false;
            _dialogs.Add(dialogId, dialog);
            return true;
        }

        public bool RemoveDialog(string dialogId)
        {
            return _dialogs.Remove(dialogId);
        }

        public bool SetDialogId(string oldDialogId, string newDialogId)
        {
            if (!HasDialog(oldDialogId))
                return false;

            if (HasDialog(newDialogId))
                return false;

            var dialog = GetDialog(oldDialogId);
            RemoveDialog(oldDialogId);
            AddDialog(newDialogId, dialog);
            return true;
        }
    }
}
