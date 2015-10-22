using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;

namespace Zelda.Editor.Modules.Shell.Commands
{
    [CommandHandler]
    class CloseFileCommandHandler : CommandHandlerBase<CloseFileCommandDefinition>
    {
        readonly IShell _shell;

        [ImportingConstructor]
        public CloseFileCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        public override void Update(Command command)
        {
            command.Enabled = _shell.ActiveDocument != null;
            base.Update(command);
        }

        public override Task Run(Command command)
        {
            _shell.CloseDocument(_shell.ActiveDocument);
            return TaskUtility.Completed;
        }
    }
}
