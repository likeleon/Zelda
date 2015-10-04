using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.DialogsEditor.Models;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.ViewModels
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
            UpdateChildIcons(_dialogTree.Root);
        }

        void UpdateChildIcons(Node node)
        {
            node.Children.Do(child => UpdateChildIcons(child));
            node.Icon = GetIcon(node);
        }

        Uri GetIcon(Node node)
        {
            if (!DialogExists(node.Key))
            {
                if (TranslatedDialogExists(node.Key))
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

        public bool DialogExists(string id)
        {
            return _resources.HasDialog(id);
        }

        public string GetDialogText(string id)
        {
            if (!DialogExists(id))
                return null;

            return _resources.GetDialog(id).Text;
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
            UpdateChildIcons(_dialogTree.Root);
        }

        public void ReloadTranslation()
        {
            ClearTranslationFromTree();

            var path = _mod.GetDialogsPath(TranslationId);
            _translationResources.Clear();
            if (!_translationResources.ImportFromFile(path))
            {
                TranslationId = null;
                throw new Exception("Cannot open dialogs data file '{0}'".F(path));
            }

            _translationResources.Dialogs.Keys.Do(dialogId => _dialogTree.AddRef(dialogId));
            UpdateChildIcons(_dialogTree.Root);
        }

        void ClearTranslationFromTree()
        {
            foreach (var id in _translationResources.Dialogs.Keys)
            {
                var node = _dialogTree.Find(id);
                if (node != null)
                    _dialogTree.RemoveRef(node, DialogExists(node.Key));
            }
        }

        public bool TranslatedDialogExists(string id)
        {
            return _translationResources.HasDialog(id);
        }

        public string GetTranslatedDialogText(string id)
        {
            if (!TranslatedDialogExists(id))
                return null;

            return _translationResources.GetDialog(id).Text;
        }

        public IReadOnlyDictionary<string, string> GetDialogProperties(string id)
        {
            if (!DialogExists(id))
                return new Dictionary<string, string>();

            return _resources.GetDialog(id).Properties;
        }

        public IReadOnlyDictionary<string, string> GetTranslatedDialogProperties(string id)
        {
            if (!TranslatedDialogExists(id))
                return new Dictionary<string, string>();

            return _translationResources.GetDialog(id).Properties;
        }
    }
}
