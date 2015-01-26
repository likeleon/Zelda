using Caliburn.Micro;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;

namespace Zelda.Editor.Modules.ErrorList.Commands
{
    [CommandHandler]
    public class ViewErrorListCommandHandler : CommandHandlerBase<ViewErrorListCommandDefinition>
    {
        private readonly IShell _shell;

        [ImportingConstructor]
        public ViewErrorListCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        public override Task Run(Command command)
        {
            _shell.ShowTool<IErrorList>();

            IErrorList errorList = IoC.Get<IErrorList>();
            errorList.AddItem(ErrorListItemType.Message, "Test message", "asset/contents", 30, 20);

            return TaskUtility.Completed;
        }
    }
}
