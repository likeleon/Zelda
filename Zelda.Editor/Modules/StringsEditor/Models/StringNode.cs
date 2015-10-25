using System;
using Zelda.Editor.Core;
using Zelda.Editor.Modules.DialogsEditor.Models;

namespace Zelda.Editor.Modules.StringsEditor.Models
{
    class StringNode : Node
    {
        Uri _icon;
        string _value;

        public Uri Icon
        {
            get { return _icon; }
            set { this.SetProperty(ref _icon, value); }
        }

        public string Value
        {
            get { return _value; }
            set { this.SetProperty(ref _value, value); }
        }
    }
}
