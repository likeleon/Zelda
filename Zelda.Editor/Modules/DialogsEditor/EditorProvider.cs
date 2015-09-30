using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.DialogsEditor.ViewModels;

namespace Zelda.Editor.Modules.DialogsEditor
{
    [Export(typeof(IEditorProvider))]
    class EditorProvider : IEditorProvider
    {
        readonly IModService _modService;

        public IDocument Create() { return new EditorViewModel(_modService.Mod); }

        public IDocument Find(IEnumerable<IDocument> documents, string path)
        {
            return documents.OfType<EditorViewModel>().FirstOrDefault(vm => vm.FilePath == path);
        }

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
            await ((EditorViewModel)document).Load(path);
        }
    }
}
