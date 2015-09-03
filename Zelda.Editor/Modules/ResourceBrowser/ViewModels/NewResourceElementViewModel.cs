using System;
using System.ComponentModel;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser.ViewModels
{
    class NewResourceElementViewModel : WindowBase, IDataErrorInfo
    {
        public string Title { get; private set; }
        public string IdLabel { get; private set; }
        public string Id { get; set; }
        public string Description { get; set; }

        public string Error { get { return null; } }

        public RelayCommand OkCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "Id":
                        if (Id.IsNullOrEmpty())
                            return "Empty resource id";
                        if (!_mod.IsValidFileName(Id))
                            return "Invalid resource id";
                        break;

                    case "Description":
                        if (Description.IsNullOrEmpty())
                            return "Empty resource description";
                        break;
                }
                return string.Empty;
            }
        }

        readonly IMod _mod;

        public NewResourceElementViewModel(string resourceTypeName, IMod mod)
        {
            if (mod == null)
                throw new ArgumentNullException("mod");

            _mod = mod;

            Title = "New {0}".F(resourceTypeName.ToLower());
            IdLabel = "{0} id (filename):".F(resourceTypeName);

            OkCommand = new RelayCommand(o => TryClose(true));
            CancelCommand = new RelayCommand(o => TryClose(false));
        }
    }
}
