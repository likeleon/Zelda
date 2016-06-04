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

namespace Zelda.Editor.Modules.StringsEditor.Models
{
    // TODO: String 리소스 Key 정렬로 저장
    class StringsModel : PropertyChangedBase
    {
        public event EventHandler<string> StringCreated;
        public event EventHandler<StringKeyChangedEventArgs> StringKeyChanged;
        public event EventHandler<string> StringDeleted;
        public event EventHandler<StringValueChangedEventArgs> StringValueChanged;

        readonly IMod _mod;
        readonly string _languageId;
        readonly StringResources _resources = new StringResources();
        StringResources _translationResources;
        string _translationId;

        public NodeTree<StringNode> StringTree { get; private set; }

        public string TranslationId
        {
            get { return _translationId; }
            set { this.SetProperty(ref _translationId, value); }
        }

        public StringsModel(IMod mod, string languageId)
        {
            if (mod == null)
                throw new ArgumentNullException("mod");

            if (languageId.IsNullOrEmpty())
                throw new ArgumentNullException("languageId");

            _mod = mod;
            _languageId = languageId;

            var path = _mod.GetStringsPath(languageId);
            _resources = XmlLoader.Load<StringResources>(path);

            BuildStringTree();
        }

        void BuildStringTree()
        {
            StringTree = new NodeTree<StringNode>(".");
            foreach (var kvp in _resources.Strings)
            {
                var node = StringTree.AddKey(kvp.Key);
                node.Value = kvp.Value;
            }
            UpdateChildIcons(StringTree.Root);
        }

        void UpdateChildIcons(StringNode node)
        {
            node.Children.Values.Do(child => UpdateChildIcons((StringNode)child));
            UpdateIcon(node);
        }

        void UpdateIcon(StringNode node)
        {
            if (!StringExists(node.Key))
            {
                if (TranslatedStringExists(node.Key))
                {
                    if (node.Children.Any())
                        node.Icon = "icon_strings_missing.png".ToIconUri();
                    else
                        node.Icon = "icon_string_missing.png".ToIconUri();
                }
                else
                    node.Icon = "icon_folder_open.png".ToIconUri();
            }
            else
            {
                if (node.Children.Any())
                    node.Icon = "icon_strings.png".ToIconUri();
                else
                    node.Icon = "icon_string.png".ToIconUri();
            }
        }

        public void Save()
        {
            var path = _mod.GetStringsPath(_languageId);
            XmlSaver.Save(_resources, path);
        }

        public bool StringExists(string key)
        {
            return _resources.HasString(key);
        }

        public bool TranslatedStringExists(string key)
        {
            return _translationResources?.HasString(key) ?? false;
        }

        public string GetString(string key)
        {
            if (!StringExists(key))
                return null;

            return _resources.GetString(key);
        }

        public void CreateString(string key, string value)
        {
            if (!IsValidKey(key))
                throw new InvalidOperationException("Invalid string key: {0}".F(key));

            if (StringExists(key))
                throw new InvalidOperationException("String '{0}' already exists".F(key));

            _resources.AddString(key, value);
            var node = StringTree.AddKey(key);
            node.Value = value;
            UpdateIcon(node);

            if (StringCreated != null)
                StringCreated(this, key);
        }

        public static bool IsValidKey(string key)
        {
            return !key.IsNullOrEmpty() && !key.StartsWith(".") && !key.EndsWith(".");
        }

        public void DeleteString(string key)
        {
            if (!StringExists(key))
                throw new InvalidOperationException("Invalid string key: '{0}'".F(key));

            _resources.RemoveString(key);

            var node = StringTree.Find(key);
            StringTree.RemoveKey(node);
            node.Value = null;
            UpdateIcon(node);

            if (StringDeleted != null)
                StringDeleted(this, key);
        }

        public void SetString(string key, string value)
        {
            if (GetString(key) == value)
                return;

            _resources.SetString(key, value);

            var node = StringTree.Find(key);
            node.Value = value;
            UpdateIcon(node);

            if (StringValueChanged != null)
                StringValueChanged(this, new StringValueChangedEventArgs(key, value));
        }

        public void SetTranslationId(string languageId)
        {
            LoadTranslation(languageId);
        }

        public void ReloadTranslation()
        {
            if (TranslationId == null)
                throw new InvalidOperationException("TranslationId is null");

            LoadTranslation(TranslationId);
        }

        void LoadTranslation(string languageId)
        {
            ClearTranslation();

            var path = _mod.GetStringsPath(languageId);
            _translationResources = XmlLoader.Load<StringResources>(path);
            _translationResources.Strings.Keys.Do(stringKey => StringTree.AddRef(stringKey));
            UpdateChildIcons(StringTree.Root);
            TranslationId = languageId;
        }

        public string[] GetKeys(string prefix)
        {
            return _resources.Strings.Keys.Where(key => key.StartsWith(prefix)).ToArray();
        }

        public bool PrefixExists(string prefix)
        {
            return _resources.Strings.Keys.Any(key => key.StartsWith(prefix));
        }

        public void ClearTranslation()
        {
            ClearTranslationFromTree();
            _translationResources?.Clear();
            UpdateChildIcons(StringTree.Root);
            TranslationId = null;
        }

        void ClearTranslationFromTree()
        {
            foreach (var id in _translationResources?.Strings.Keys)
            {
                var node = StringTree.Find(id);
                if (node != null)
                    StringTree.RemoveRef(node, StringExists(node.Key));
            }
        }

        public string GetTranslatedString(string key)
        {
            if (!TranslatedStringExists(key))
                return null;

            return _translationResources.GetString(key);
        }

        public bool CanSetStringKeyPrefix(string oldPrefix, string newPrefix, out string key)
        {
            foreach (var prefixedKey in GetKeys(oldPrefix))
            {
                var newKey = Regex.Replace(prefixedKey, "^" + oldPrefix, newPrefix);
                if (!StringExists(newKey))
                {
                    key = newKey;
                    return false;
                }
            }
            key = null;
            return true;
        }

        public List<Tuple<string, string>> SetStringKeyPrefix(string oldPrefix, string newPrefix)
        {
            string key;
            if (!CanSetStringKeyPrefix(oldPrefix, newPrefix, out key))
                throw new InvalidOperationException("String '{0}' already exists".F(key));

            var list = new List<Tuple<string, string>>();
            foreach (var oldKey in GetKeys(oldPrefix))
            {
                var newKey = SetStringKey(oldKey, Regex.Replace(oldKey, "^" + oldKey, newPrefix));
                list.Add(Tuple.Create(oldKey, newKey));
            }
            return list;
        }

        public string SetStringKey(string key, string newKey)
        {
            if (newKey == key)
                return key;

            if (!StringExists(key))
                throw new InvalidOperationException("String '{0}' no exists".F(key));

            if (StringExists(newKey))
                throw new InvalidOperationException("String '{0}' already exists".F(newKey));

            if (!IsValidKey(newKey))
                throw new InvalidOperationException("Invalid String id: {0}".F(newKey));

            _resources.SetStringKey(key, newKey);
            StringTree.RemoveKey(StringTree.Find(key));
            var node = StringTree.AddKey(newKey);
            UpdateIcon(node);

            StringKeyChanged?.Invoke(this, new StringKeyChangedEventArgs(key, newKey));
            return newKey;
        }
    }

    class StringValueChangedEventArgs : EventArgs
    {
        public string Key { get; private set; }
        public string NewValue { get; private set; }

        public StringValueChangedEventArgs(string key, string newValue)
        {
            Key = key;
            NewValue = newValue;
        }
    }

    class StringKeyChangedEventArgs : EventArgs
    {
        public string OldKey { get; private set; }
        public string NewKey { get; private set; }

        public StringKeyChangedEventArgs(string oldKey, string newkey)
        {
            OldKey = oldKey;
            NewKey = newkey;
        }
    }
}
