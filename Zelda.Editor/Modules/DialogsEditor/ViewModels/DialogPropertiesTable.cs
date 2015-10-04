using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Services;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.ViewModels
{
    class DialogPropertiesTable : PropertyChangedBase
    {
        public class Item : PropertyChangedBase
        {
            string _value;
            Uri _icon;
            string _translation;

            public string Key { get; private set; }
            public string Value
            {
                get { return _value; }
                set { this.SetProperty(ref _value, value); }
            }
            public Uri Icon
            {
                get { return _icon; }
                set { this.SetProperty(ref _icon, value); }
            }

            public string Translation
            {
                get { return _translation; }
                set { this.SetProperty(ref _translation, value); }
            }

            public Item(string key)
            {
                Key = key;
            }
        }

        readonly DialogsModel _dialogsModel;
        readonly ObservableCollection<Item> _items = new ObservableCollection<Item>();
        string _dialogsId;
        bool _isTranslationColumnVisible;

        public string DialogId
        {
            get { return _dialogsId; }
            set
            {
                if (this.SetProperty(ref _dialogsId, value))
                    Update();
            }
        }

        public IEnumerable<Item> Items { get { return _items; } }

        public bool IsTranslationColumnVisible
        {
            get { return _isTranslationColumnVisible; }
            set { this.SetProperty(ref _isTranslationColumnVisible, value); }
        }

        public DialogPropertiesTable(DialogsModel dialogsModel)
        {
            if (dialogsModel == null)
                throw new ArgumentNullException("dialogsModel");

            _dialogsModel = dialogsModel;
        }

        void Update()
        {
            ClearTable();

            if (DialogId == null)
                return;

            var properties = _dialogsModel.GetDialogProperties(DialogId);
            properties.Do(kvp => DialogPropertyCreated(DialogId, kvp.Key, kvp.Value));

            if (!_dialogsModel.TranslatedDialogExists(DialogId))
            {
                IsTranslationColumnVisible = false;
                return;
            }

            IsTranslationColumnVisible = true;
            properties = _dialogsModel.GetTranslatedDialogProperties(DialogId);
            properties.Do(kvp => AddTranslationProperty(kvp.Key, kvp.Value));
        }

        void ClearTable()
        {
            _items.Clear();
        }

        void DialogPropertyCreated(string id, string key, string value)
        {
            if (id != DialogId)
                return;

            var item = _items.FirstOrDefault(i => i.Key == key);
            if (item == null)
            {
                item = new Item(key);
                _items.Add(item);
            }

            item.Icon = "icon_property.png".ToIconUri();
            item.Value = value;
        }

        void AddTranslationProperty(string key, string value)
        {
            var item = _items.FirstOrDefault(i => i.Key == key);
            if (item != null)
                item.Translation = value;
            else
            {
                item = new Item(key) { Translation = value };
                _items.Add(item);
            }
        }
    }
}
