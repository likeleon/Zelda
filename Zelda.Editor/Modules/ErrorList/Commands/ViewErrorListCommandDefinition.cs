using System;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;

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
                return "BuildErrorList_7237.png".ToIconUri();
            }
        }
    }
}
