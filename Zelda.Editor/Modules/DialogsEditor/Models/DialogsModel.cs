using System;
using Zelda.Editor.Core.Mods;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.Models
{
    class DialogsModel
    {
        readonly IMod _mod;
        readonly string _languageId;
        readonly DialogResources _resources = new DialogResources();
        readonly NodeTree<Dialog> _dialogTree = new NodeTree<Dialog>(".");

        public Dialog Root { get { return _dialogTree.Root; } }

        public DialogsModel(IMod mod, string languageId)
        {
            if (mod == null)
                throw new ArgumentNullException("mod");

            if (languageId.IsNullOrEmpty())
                throw new ArgumentNullException("languageId");

            _mod = mod;
            _languageId = languageId;

            var path = _mod.GetDialogsPath(languageId);
            if (!_resources.ImportFromFile(path))
                throw new Exception("Cannot open dialogs data file '{0}'".F(path));

            BuildDialogTree();
        }

        void BuildDialogTree()
        {
            _dialogTree.Clear();
            _resources.Dialogs.Keys.Do(id => _dialogTree.AddKey(id));
        }
    }
}
