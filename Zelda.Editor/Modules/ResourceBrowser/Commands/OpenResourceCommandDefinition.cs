using System;
using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Modules.ResourceBrowser.Commands
{
    [CommandDefinition]
    class OpenResourceCommandDefinition : CommandDefinition
    {
        string _text = "_Open";
        string _toolTip = "Open";
        Uri _iconSource;

        public const string CommandName = "ResourceBrowser.OpenResource";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "_Open"; } }
        public override string ToolTip { get { return "Open"; } }
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
