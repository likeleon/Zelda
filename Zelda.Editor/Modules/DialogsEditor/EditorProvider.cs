using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;
using Zelda.Editor.Modules.DialogsEditor.ViewModels;

namespace Zelda.Editor.Modules.DialogsEditor
{
    [Export(typeof(IEditorProvider))]
    class EditorProvider : IEditorProvider
    {
        readonly IModService _modService;

        public IDocument Create() { return new EditorViewModel(); }

        [ImportingConstructor]
        public EditorProvider(IModService modService)
        {
            _modService = modService;
        }

        public bool Handles(string path)
        {
            var mod = _modService.Mod;
            if (mod == null)
                return false;

            var languageId = "";
            return mod.IsDialogsFile(path, ref languageId);
        }

        public async Task Open(IDocument document, string path)
        {
            //await ((EditorViewModel)document).Load(path);
            await TaskUtility.Completed;
        }
    }
}
