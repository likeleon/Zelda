using System;
using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Modules.ResourceBrowser.Commands
{
    [CommandDefinition]
    class NewResourceElementCommandDefinition : CommandDefinition
    {
        string _text;
        string _toolTip;
        Uri _iconSource;

        public const string CommandName = "ResourceBrowser.NewResourceElement";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return _text; } }
        public override string ToolTip { get { return _text; } }
        public override Uri IconSource { get { return _iconSource; } }

        public void SetText(string text)
        {
            _text = text;
        }

        public void SetToolTip(string toolTip)
        {
            _toolTip = toolTip;
        }

        public void SetIconSource(Uri uri)
        {
            _iconSource = uri;
        }
    }
}
