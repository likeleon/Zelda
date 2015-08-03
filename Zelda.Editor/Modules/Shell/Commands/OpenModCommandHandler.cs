using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;

namespace Zelda.Editor.Modules.Shell.Commands
{
    [CommandHandler]
    class OpenModCommandHandler : CommandHandlerBase<OpenModCommandDefinition>
    {
        readonly IMod _mod;

        [ImportingConstructor]
        public OpenModCommandHandler(IMod mod)
        {
            _mod = mod;
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
                _mod.Load(modPath);
            }
            catch (Exception e)
            {
                e.ShowDialog();
            }
        }
    }
}
    