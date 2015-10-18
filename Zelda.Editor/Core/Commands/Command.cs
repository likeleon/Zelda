using Caliburn.Micro;
using System;
using System.Windows.Input;

namespace Zelda.Editor.Core.Commands
{
    public class Command : PropertyChangedBase
    {
        readonly CommandDefinitionBase _commandDefinition;
        bool _visible = true;
        bool _enabled = true;
        bool _checked;
        string _text;
        string _toolTip;
        Uri _iconSource;
        readonly KeyGesture _keyGesture;

        public CommandDefinitionBase CommandDefinition
        {
            get { return _commandDefinition; }
        }

        public bool Visible
        {
            get { return _visible; }
            set { this.SetProperty(ref _visible, value); }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { this.SetProperty(ref _enabled, value); }
        }

        public bool Checked
        {
            get { return _checked; }
            set { this.SetProperty(ref _checked, value); }
        }

        public string Text
        {
            get { return _text; }
            set { this.SetProperty(ref _text, value); }
        }

        public string ToolTip
        {
            get { return _toolTip; }
            set { this.SetProperty(ref _toolTip, value); }
        }

        public Uri IconSource
        {
            get { return _iconSource; }
            set { this.SetProperty(ref _iconSource, value); }
        }

        public KeyGesture KeyGesture
        {
            get { return _keyGesture; }
        }

        public object Tag { get; set; }

        public Command(CommandDefinitionBase commandDefinition)
        {
            _commandDefinition = commandDefinition;
            Text = commandDefinition.Text;
            ToolTip = commandDefinition.ToolTip;
            IconSource = commandDefinition.IconSource;
            _keyGesture = commandDefinition.KeyGesture;
        }
    }
}
