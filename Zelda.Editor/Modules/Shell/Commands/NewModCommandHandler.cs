using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;

namespace Zelda.Editor.Modules.Shell.Commands
{
    [CommandHandler]
    class NewModCommandHandler : CommandHandlerBase<NewModCommandDefinition>
    {
        readonly IShell _shell;

        [ImportingConstructor]
        public NewModCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        public override Task Run(Command command)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Select mod directory";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            dlg.DefaultDirectory = dlg.InitialDirectory;
            dlg.EnsurePathExists = true;
            if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                return TaskUtility.Completed;

            return TaskUtility.Completed;
        }
    }
}
