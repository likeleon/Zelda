using System;
using Zelda.Editor.Core;

namespace Zelda.Editor.Modules.DialogsEditor.Models
{
    class DialogNode : Node
    {
        Uri _icon;

        public Uri Icon
        {
            get { return _icon; }
            set { this.SetProperty(ref _icon, value); }
        }
    }
}
