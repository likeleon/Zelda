using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Zelda.Game.Engine;

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

    class DialogResource
    {
        static readonly string _fileName = "text/dialogs.xml";
        static readonly Dictionary<string, Dialog> _dialogs = new Dictionary<string, Dialog>();

        public static void Initialize()
        {
            _dialogs.Clear();

            var buffer = ModFiles.DataFileRead(_fileName, true);
            var data = buffer.XmlDeserialize<DialogXmlData>();
            foreach (var dialogData in data.Dialogs.EmptyIfNull())
            {
                Dialog dialog = new Dialog();
                dialog.Id = dialogData.Id;
                dialog.Text = dialogData.Text;

                if (dialogData.PropertyElements != null)
                {
                    foreach (XmlElement element in dialogData.PropertyElements)
                        dialog.SetProperty(element.Name, element.InnerText);
                }

                if (String.IsNullOrEmpty(dialog.Id))
                    throw new InvalidDataException("Missing value dialog Id");

                if (string.IsNullOrEmpty(dialog.Text))
                    throw new InvalidDataException("Missing text for dialog '{0}'".F(dialog.Id));

                if (Exists(dialog.Id))
                    throw new InvalidDataException("Duplicate dialog '{0}'".F(dialog.Id));

                _dialogs.Add(dialog.Id, dialog);
            }
        }

        public static void Quit()
        {
            _dialogs.Clear();
        }

        public static bool Exists(string dialogId)
        {
            return _dialogs.ContainsKey(dialogId);
        }

        public static Dialog GetDialog(string dialogId)
        {
            Debug.CheckAssertion(Exists(dialogId), "No such dialog: '{0}'".F(dialogId));
            return _dialogs[dialogId];
        }
    }
}
