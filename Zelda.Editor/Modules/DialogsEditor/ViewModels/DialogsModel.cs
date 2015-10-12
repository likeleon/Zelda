using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.DialogsEditor.Models;
using Zelda.Editor.Modules.Mods.Models;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.ViewModels
{
    class DialogsModel : PropertyChangedBase
    {
        public event EventHandler<string> DialogCreated;
        public event EventHandler<DialogIdChangedEventArgs> DialogIdChanged;
        public event EventHandler<string> DialogDeleted;
        public event EventHandler<DialogPropertyEventArgs> DialogPropertyCreated;
        public event EventHandler<DialogPropertyEventArgs> DialogPropertyChanged;
        public event EventHandler<DialogPropertyEventArgs> DialogPropertyDeleted;

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

        public void Save()
        {
            var path = _mod.GetDialogsPath(_languageId);
            if (!_resources.ExportToFile(path))
                throw new Exception("Cannot save dialogs data file '{0}'".F(path));
        }

        public bool DialogExists(string id)
        {
            return _resources.HasDialog(id);
        }

        public bool DialogPropertyExists(string id, string key)
        {
            return DialogExists(id) &&
                   _resources.GetDialog(id).HasProperty(key);
        }

        public bool PrefixExists(string prefix)
        {
            return _resources.Dialogs.Keys.Any(key => key.StartsWith(prefix));
        }

        public string[] GetIds(string prefix)
        {
            return _resources.Dialogs.Keys.Where(key => key.StartsWith(prefix)).ToArray();
        }

        public DialogData GetDialogData(string id)
        {
            if (!DialogExists(id))
                return null;
            return _resources.GetDialog(id);
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

        public Node FindNode(string id)
        {
            return _dialogTree.Find(id);
        }

        public void CreateDialog(string id, string text, Dictionary<string, string> properties = null)
        {
            var data = new DialogData()
            {
                Id = id,
                Text = text
            };
            if (properties != null)
                properties.Do(kvp => data.SetProperty(kvp.Key, kvp.Value));
            CreateDialog(id, data);
        }

        public void CreateDialog(string id, DialogData data)
        {
            if (!IsValidId(id))
                throw new InvalidOperationException("Invalid dialog id: '{0}'".F(id));

            if (DialogExists(id))
                throw new InvalidOperationException("Dialog '{0}' already exists".F(id));

            _resources.AddDialog(id, data);
            var node = _dialogTree.AddKey(id);
            node.Icon = GetIcon(node);

            if (DialogCreated != null)
                DialogCreated(this, id);
        }

        public static bool IsValidId(string id)
        {
            return !id.IsNullOrEmpty() && !id.StartsWith(".") && !id.EndsWith(".");
        }

        public string SetDialogId(string id, string newId)
        {
            if (newId == id)
                return id;

            if (!DialogExists(id))
                throw new InvalidOperationException("Dialog '{0}' no exists".F(id));

            if (DialogExists(newId))
                throw new InvalidOperationException("Dialog '{0}' already exists".F(newId));

            if (!IsValidId(newId))
                throw new InvalidOperationException("Invalid dialog id: {0}".F(newId));

            _resources.SetDialogId(id, newId);
            _dialogTree.RemoveKey(_dialogTree.Find(id));
            var node = _dialogTree.AddKey(newId);
            node.Icon = GetIcon(node);

            if (DialogIdChanged != null)
                DialogIdChanged(this, new DialogIdChangedEventArgs(id, newId));

            return newId;
        }

        public bool CanSetDialogIdPrefix(string oldPrefix, string newPrefix, out string id)
        {
            foreach (var prefixedId in GetIds(oldPrefix))
            {
                var newId = Regex.Replace(prefixedId, "^" + oldPrefix, newPrefix);
                if (DialogExists(newId))
                {
                    id = newId;
                    return false;
                }
            }
            id = null;
            return true;
        }

        public List<Tuple<string, string>> SetDialogIdPrefix(string oldPrefix, string newPrefix)
        {
            string id;
            if (!CanSetDialogIdPrefix(oldPrefix, newPrefix, out id))
                throw new InvalidOperationException("Dialog '{0}' already exists".F(id));

            var list = new List<Tuple<string, string>>();
            foreach (var oldId in GetIds(oldPrefix))
            {
                var newId = Regex.Replace(oldId, "^" + oldPrefix, newPrefix);
                list.Add(Tuple.Create(oldId, SetDialogId(oldId, newId)));
            }
            return list;
        }

        public void DeleteDialog(string id)
        {
            if (!DialogExists(id))
                throw new InvalidOperationException("Invalid dialog id: '{0}'".F(id));

            _resources.RemoveDialog(id);
            _dialogTree.RemoveKey(_dialogTree.Find(id));

            if (DialogDeleted != null)
                DialogDeleted(this, id);
        }

        public List<Tuple<string, DialogData>> DeletePrefix(string prefix)
        {
            var list = new List<Tuple<string, DialogData>>();
            foreach (var key in GetIds(prefix))
            {
                list.Add(Tuple.Create(key, GetDialogData(key)));
                DeleteDialog(key);
            }
            return list;
        }

        public void SetDialogProperty(string id, string key, string value)
        {
            var exists = DialogPropertyExists(id, key);
            if (exists && GetDialogProperty(id, key) == value)
                return;

            _resources.GetDialog(id).SetProperty(key, value);
            if (exists)
            {
                if (DialogPropertyChanged != null)
                    DialogPropertyChanged(this, new DialogPropertyEventArgs(id, key, value));
            }
            else
            {
                if (DialogPropertyCreated != null)
                    DialogPropertyCreated(this, new DialogPropertyEventArgs(id, key, value));
            }
        }

        public string GetDialogProperty(string id, string key)
        {
            if (!DialogPropertyExists(id, key))
                return null;

            return _resources.GetDialog(id).GetProperty(key);
        }

        public void DeleteDialogProperty(string id, string key)
        {
            if (!DialogPropertyExists(id, key))
                return;

            _resources.GetDialog(id).RemoveProperty(key);

            if (DialogPropertyDeleted != null)
                DialogPropertyDeleted(this, new DialogPropertyEventArgs(id, key, null));
        }
    }

    class DialogIdChangedEventArgs : EventArgs
    {
        public string OldId { get; private set; }
        public string NewId { get; private set; }

        public DialogIdChangedEventArgs(string oldId, string newId)
        {
            OldId = oldId;
            NewId = newId;
        }
    }

    class DialogPropertyEventArgs : EventArgs
    {
        public string Id { get; private set; }
        public string Key { get; private set; }
        public string Value { get; private set; }

        public DialogPropertyEventArgs(string id, string key, string value)
        {
            Id = id;
            Key = key;
            Value = value;
        }
    }
}
