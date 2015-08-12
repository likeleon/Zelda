using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Core.Threading;

namespace Zelda.Editor.Modules.Shell.Commands
{
    [CommandHandler]
    class OpenModCommandHandler : CommandHandlerBase<OpenModCommandDefinition>
    {
        readonly IModService _modService;

        [ImportingConstructor]
        public OpenModCommandHandler(IModService modService)
        {
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
                if (_modService.IsLoaded)
                    _modService.Unload();

                _modService.Load(modPath);
            }
            catch (Exception e)
            {
                e.ShowDialog();
            }
        }
    }
}
    