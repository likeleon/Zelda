using System;
using System.Globalization;
using Zelda.Editor.Core.Menus;

namespace Zelda.Editor.Modules.MainMenu.Models
{
    public class TextMenuItem : StandardMenuItem
    {
        private readonly MenuDefinitionBase _menuDefinition;

        public override string Text
        {
            get { return _menuDefinition.Text; }
        }

        public override Uri IconSource
        {
            get { return _menuDefinition.IconSource; }
        }

        public override string InputGestureText
        {
            get
            {
                return _menuDefinition.KeyGesture == null
                    ? string.Empty
                    : _menuDefinition.KeyGesture.GetDisplayStringForCulture(CultureInfo.CurrentUICulture);
            }
        }

        public override System.Windows.Input.ICommand Command
        {
            get { return null; }
        }

        public override bool IsChecked
        {
            get { return false; }
        }

        public override bool IsVisible
        {
            get { return true; }
        }

        public TextMenuItem(MenuDefinitionBase menuDefinition)
        {
            _menuDefinition = menuDefinition;
        }
    }
}
