using System;
using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Modules.ErrorList.Commands
{
    [CommandDefinition]
    public class ViewErrorListCommandDefinition : CommandDefinition
    {
        public const string CommandName = "View.ErrorList";
        public override string Name

        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return "Error L_ist"; }
        }

        public override string ToolTip
        {
            get { return "Error List"; }
        }

        public override Uri IconSource
        {
            get
            {
                return new Uri("/Resources/Icons/BuildErrorList_7237.png", UriKind.Relative);
            }
        }
    }
}
