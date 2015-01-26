using System;
using System.Windows.Input;

namespace Zelda.Editor.Core.Commands
{
    public abstract class CommandDefinition : CommandDefinitionBase
    {
        public override Uri IconSource
        {
            get { return null; }
        }

        public override KeyGesture KeyGesture
        {
            get { return null; }
        }
    }
}
