using System;
using Zelda.Editor.Core;
using Zelda.Editor.Modules.DialogsEditor.Models;

namespace Zelda.Editor.Modules.StringsEditor.Models
{
    class StringNode : Node
    {
        Uri _icon;

        public Uri Icon
        {
            get { return _icon; }
            set { this.SetProperty(ref _icon, value); }
        }
    }
}
