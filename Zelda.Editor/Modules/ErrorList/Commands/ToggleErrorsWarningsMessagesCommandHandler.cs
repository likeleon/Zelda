using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Threading;
using Zelda.Game;

namespace Zelda.Editor.Modules.ErrorList.Commands
{
    [CommandHandler]
    class ToggleErrorsWarningsMessagesCommandHandler :
        ICommandHandler<ToggleErrorsCommandDefinition>,
        ICommandHandler<ToggleWarningsCommandDefinition>,
        ICommandHandler<ToggleMessagesCommandDefinition>
    {
        readonly IErrorList _errorList;

        int ErrorItemCount { get { return _errorList.Items.Count(x => x.ItemType == ErrorListItemType.Error); } }
        int WarningItemCount { get { return _errorList.Items.Count(x => x.ItemType == ErrorListItemType.Warning); } }
        int MessageItemCount { get { return _errorList.Items.Count(x => x.ItemType == ErrorListItemType.Message); } }

        [ImportingConstructor]
        public ToggleErrorsWarningsMessagesCommandHandler(IErrorList errorList)
        {
            _errorList = errorList;
        }

        void ICommandHandler<ToggleErrorsCommandDefinition>.Update(Command command)
        {
            command.Enabled = ErrorItemCount > 0;
            command.Checked = command.Enabled && _errorList.ShowErrors;
            command.Text = command.ToolTip = Pluralize("{0} Error", "{0} Errors", ErrorItemCount);
        }

        static string Pluralize(string singular, string plural, int number)
        {
            if (number == 1)
                return singular.F(number);

            return plural.F(number);
        }

        Task ICommandHandler<ToggleErrorsCommandDefinition>.Run(Command command)
        {
            _errorList.ShowErrors = !_errorList.ShowErrors;
            return TaskUtility.Completed;
        }

        void ICommandHandler<ToggleWarningsCommandDefinition>.Update(Command command)
        {
            command.Enabled = WarningItemCount > 0;
            command.Checked = command.Enabled && _errorList.ShowWarnings;
            command.Text = command.ToolTip = Pluralize("{0} Warning", "{0} Warnings", WarningItemCount);
        }

        Task ICommandHandler<ToggleWarningsCommandDefinition>.Run(Command command)
        {
            _errorList.ShowWarnings = !_errorList.ShowWarnings;
            return TaskUtility.Completed;
        }

        void ICommandHandler<ToggleMessagesCommandDefinition>.Update(Command command)
        {
            command.Enabled = MessageItemCount > 0;
            command.Checked = command.Enabled && _errorList.ShowMessages;
            command.Text = command.ToolTip = Pluralize("{0} Message", "{0} Messages", MessageItemCount);
        }

        Task ICommandHandler<ToggleMessagesCommandDefinition>.Run(Command command)
        {
            _errorList.ShowMessages = !_errorList.ShowMessages;
            return TaskUtility.Completed;
        }
    }
}
