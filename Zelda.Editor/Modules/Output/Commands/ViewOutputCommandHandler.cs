using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;

namespace Zelda.Editor.Modules.Output.Commands
{
    [CommandHandler]
    class ViewOutputCommandHandler : CommandHandlerBase<ViewOutputCommandDefinition>
    {
        private IShell _shell;

        [ImportingConstructor]
        public ViewOutputCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        public override Task Run(Command command)
        {
            _shell.ShowTool<IOutput>();
            return TaskUtility.Completed;
        }
    }
}
