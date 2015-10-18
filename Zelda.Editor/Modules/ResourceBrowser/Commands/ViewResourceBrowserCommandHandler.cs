using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;

namespace Zelda.Editor.Modules.ResourceBrowser.Commands
{
    [CommandHandler]
    class ViewResourceBrowserCommandHandler : CommandHandlerBase<ViewResourceBrowserCommandDefinition>
    {
        private IShell _shell;

        [ImportingConstructor]
        public ViewResourceBrowserCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        public override Task Run(Command command)
        {
            _shell.ShowTool<IResourceBrowser>();
            return TaskUtility.Completed;
        }
    }
}
