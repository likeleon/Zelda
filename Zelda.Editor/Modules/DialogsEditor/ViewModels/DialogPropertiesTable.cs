using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zelda.Editor.Core;

namespace Zelda.Editor.Modules.DialogsEditor.ViewModels
{
    class DialogPropertiesTable : PropertyChangedBase
    {
        public class Item : PropertyChangedBase
        {
            string _value;

            public string Key { get; private set; }
            public string Value
            {
                get { return _value; }
                set { this.SetProperty(ref _value, value); }
            }

            public Item(string key, string value)
            {
                Key = key;
                _value = value;
            } 
        }

        readonly DialogsModel _dialogsModel;
        readonly ObservableCollection<Item> _items = new ObservableCollection<Item>();
        string _dialogsId;

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
        }

        void ClearTable()
        {
            _items.Clear();
        }
    }
}
