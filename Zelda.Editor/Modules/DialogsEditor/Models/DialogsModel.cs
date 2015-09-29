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
        readonly DialogResources _translationResources = new DialogResources();
        readonly NodeTree _dialogTree = new NodeTree(".");
        string _translationId;

        public Node Root { get { return _dialogTree.Root; } }
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
            _dialogTree.Clear();
            _resources.Dialogs.Keys.Do(id => _dialogTree.AddKey(id));
        }

        public Uri GetIcon(Node node)
        {
            if (!DialogExists(node))
            {
                if (TranslatedDialogExists(node))
                {
                    if (node.BindableChildren.Any())
                        return "icon_dialogs_missing.png".ToIconUri();
                    else
                        return "icon_dialog_missing.png".ToIconUri();
                }
                return "icon_folder_open.png".ToIconUri();
            }
            else
            {
                if (node.BindableChildren.Any())
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
                return null;

            return _resources.GetDialog(node.Key).Text;
        }

        public void SetTranslationId(string languageId)
        {
            TranslationId = languageId;
            ReloadTranslation();
        }

        public void ClearTranslation()
        {
            TranslationId = null;
            ClearTranslationFromTree();
            _translationResources.Clear();
        }

        public void ReloadTranslation()
        {
            ClearTranslationFromTree();

            var path = _mod.GetDialogsPath(TranslationId);
            if (!_translationResources.ImportFromFile(path))
            {
                TranslationId = null;
                throw new Exception("Cannot open dialogs data file '{0}'".F(path));
            }

            _translationResources.Dialogs.Keys.Do(dialogId => _dialogTree.AddRef(dialogId));
        }

        void ClearTranslationFromTree()
        {
            foreach (var id in _translationResources.Dialogs.Keys)
            {
                var node = _dialogTree.Find(id);
                if (node != null)
                    _dialogTree.RemoveRef(node, DialogExists(node));
            }
        }

        public bool TranslatedDialogExists(Node node)
        {
            return _translationResources.HasDialog(node.Key);
        }

        public string GetTranslatedDialogText(Node node)
        {
            if (!TranslatedDialogExists(node))
                return null;

            return _translationResources.GetDialog(node.Key).Text;
        }
    }
}
