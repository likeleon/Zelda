using Caliburn.Micro;
using System;
using System.Globalization;
using System.Windows.Input;
using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Modules.MainMenu.Models
{
    public class CommandMenuItem : StandardMenuItem
    {
        readonly Command _command;

        public override string Text { get { return _command.Text; } }
        public override Uri IconSource { get { return _command.IconSource; } }

        public override string InputGestureText
        {
            get 
            { 
                return _command.KeyGesture == null
                    ? String.Empty
                    : _command.KeyGesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture);
            }
        }

        public override ICommand Command
        {
            get { return IoC.Get<ICommandService>().GetTargetableCommand(_command); }
        }

        public override bool IsChecked { get { return _command.Checked; } }
        public override bool IsVisible { get { return _command.Visible; } }

        public CommandMenuItem(Command command)
        {
            _command = command;
        }
    }
}
