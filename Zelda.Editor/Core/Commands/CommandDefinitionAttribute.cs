using System.ComponentModel.Composition;

namespace Zelda.Editor.Core.Commands
{
    [MetadataAttribute]
    public class CommandDefinitionAttribute : ExportAttribute
    {
        public CommandDefinitionAttribute()
            : base(typeof(CommandDefinitionBase))
        {
        }
    }
}
