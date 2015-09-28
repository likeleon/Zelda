using Caliburn.Micro;
using System;
using System.Linq;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Core.Services;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.Models
{
    class DialogsModel : PropertyChangedBase
    {
        readonly IMod _mod;
        readonly string _languageId;
        readonly DialogResources _resources = new DialogResources();
        readonly NodeTree _nodeTree = new NodeTree(".");
        string _translationId;

        public Node Root { get { return _nodeTree.Root; } }
        public string TranslationId
        {
            get { return _translationId; }
            set { this.SetProperty(ref _translationId, value); }
        }

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
            _nodeTree.Clear();
            _resources.Dialogs.Keys.Do(id => _nodeTree.AddKey(id));
        }

        public Uri GetIcon(Node node)
        {
            if (!DialogExists(node))
                return "icon_folder_open.png".ToIconUri();
            else
            {
                if (node.Children.Any())
                    return "icon_dialogs.png".ToIconUri();
                else
                    return "icon_dialog.png".ToIconUri();
            }
        }

        public bool DialogExists(Node node)
        {
            return _resources.HasDialog(node.Key);
        }

        public string GetDialogText(Node node)
        {
            if (!DialogExists(node))
                return "";

            return _resources.GetDialog(node.Key).Text;
        }

        public void SetTranslation(string languageId)
        {
            TranslationId = languageId;
            ReloadTranslation();
        }

        public void ClearTranslation()
        {
            TranslationId = null;
        }

        public void ReloadTranslation()
        {
        }
    }
}
