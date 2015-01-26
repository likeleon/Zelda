using Caliburn.Micro;
using Zelda.Editor.Core;

namespace Zelda.Editor.Modules.ErrorList
{
    public class ErrorListItem : PropertyChangedBase
    {
        private ErrorListItemType _itemType;
        public ErrorListItemType ItemType
        {
            get { return _itemType; }
            set { this.SetProperty(ref _itemType, value);  }
        }

        private int _number;
        public int Number
        {
            get { return _number; }
            set { this.SetProperty(ref _number, value); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { this.SetProperty(ref _description, value); }
        }

        private string _path;
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

        private int? _line;
        public int? Line
        {
            get { return _line; }
            set { this.SetProperty(ref _line, value); }
        }

        private int? _column;
        public int? column
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
