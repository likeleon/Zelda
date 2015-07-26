using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;
using Zelda.Editor.Modules.ModEditor.Services;

namespace Zelda.Editor.Modules.ModEditor.Commands
{
    [CommandHandler]
    class OpenModCommandHandler : CommandHandlerBase<OpenModCommandDefinition>
    {
        readonly IShell _shell;
        readonly IModService _modService;

        [ImportingConstructor]
        public OpenModCommandHandler(IShell shell, IModService modService)
        {
            _shell = shell;
            _modService = modService;
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

            OpenMod(dlg.FileName);
            return TaskUtility.Completed;
        }

        void OpenMod(string modPath)
        {
            try
            {
                _modService.LoadMod(modPath);
            }
            catch (Exception e)
            {
                e.ShowDialog();
            }
        }
    }
}
    