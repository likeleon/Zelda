using Caliburn.Micro;
using Zelda.Editor.Core;

namespace Zelda.Editor.Modules.ErrorList
{
    public class ErrorListItem : PropertyChangedBase
    {
        ErrorListItemType _itemType;
        int _number;
        string _description;
        string _path;
        int? _line;
        int? _column;

        public ErrorListItemType ItemType
        {
            get { return _itemType; }
            set { this.SetProperty(ref _itemType, value);  }
        }

        public int Number
        {
            get { return _number; }
            set { this.SetProperty(ref _number, value); }
        }

        public string Description
        {
            get { return _description; }
            set { this.SetProperty(ref _description, value); }
        }

        public string Path
        {
            get { return _path; }
            set 
            {
                if (this.SetProperty(ref _path, value))
                    NotifyOfPropertyChange(() => File);
            }
        }

        public string File
        {
            get { return System.IO.Path.GetFileName(Path); }
        }

        public int? Line
        {
            get { return _line; }
            set { this.SetProperty(ref _line, value); }
        }

        public int? Column
        {
            get { return _column; }
            set { this.SetProperty(ref _column, value); }
        }

        public System.Action OnClick
        {
            get;
            set;
        }

        public ErrorListItem(ErrorListItemType itemType, int number, string description,
            string path = null, int? line = null, int? column = null)
        {
            _itemType = itemType;
            _number = number;
            _description = description;
            _path = path;
            _line = line;
            _column = column;
        }

        public ErrorListItem()
        {
        }
    }
}
