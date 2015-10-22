using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.Mods.Services;
using Zelda.Editor.Modules.StringsEditor.ViewModels;

namespace Zelda.Editor.Modules.StringsEditor
{
    [Export(typeof(IEditorProvider))]
    class EditorProvider : IEditorProvider
    {
        readonly IModService _modService;

        public IDocument Create() { throw new NotSupportedException("Creating empty string editor is impossible"); }

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
            return mod.IsStringsFile(path, ref languageId);
        }

        public IDocument Open(string path)
        {
            return new EditorViewModel(_modService.Mod, path);
        }
    }
}
