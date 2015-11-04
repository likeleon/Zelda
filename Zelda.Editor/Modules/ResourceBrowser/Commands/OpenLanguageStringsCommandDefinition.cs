using System;
using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Modules.ResourceBrowser.Commands
{
    [CommandDefinition]
    class OpenLanguageStringsCommandDefinition : CommandDefinition
    {
        Uri _iconSource;

        public const string CommandName = "ResourceBrowser.OpenLanguageStrings";
        public override string Name { get { return CommandName; } }
        public override string Text { get { return "_Open Strings"; } }
        public override string ToolTip { get { return "Open Strings"; } }
        public override Uri IconSource { get { return _iconSource; } }

        public void SetIconSource(Uri uri)
        {
            _iconSource = uri;
        }
    }
}
