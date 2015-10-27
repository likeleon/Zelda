using Caliburn.Micro;
using System;
using System.Linq;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.DialogsEditor.Models;
using Zelda.Editor.Modules.Mods.Models;
using Zelda.Game;

namespace Zelda.Editor.Modules.StringsEditor.Models
{
    class StringsModel : PropertyChangedBase
    {
        public event EventHandler<string> StringCreated;
        public event EventHandler<string> StringDeleted;
        public event EventHandler<StringValueChangedEventArgs> StringValueChanged;

        readonly IMod _mod;
        readonly string _languageId;
        readonly StringResources _resources = new StringResources();
        readonly StringResources _translationResources = new StringResources();

        public NodeTree<StringNode> StringTree { get; private set; }

        public StringsModel(IMod mod, string languageId)
        {
            if (mod == null)
                throw new ArgumentNullException("mod");

            if (languageId.IsNullOrEmpty())
                throw new ArgumentNullException("languageId");

            _mod = mod;
            _languageId = languageId;

            var path = _mod.GetStringsPath(languageId);
            if (!_resources.ImportFromFile(path))
                throw new Exception("Cannot open strings data file '{0}'".F(path));

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
            if (!_resources.ExportToFile(path))
                throw new Exception("Cannot save strings data file '{0}'".F(path));
        }

        public bool StringExists(string key)
        {
            return _resources.HasString(key);
        }

        public bool TranslatedStringExists(string key)
        {
            return _translationResources.HasString(key);
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
            UpdateIcon(node);

            if (StringCreated != null)
                StringCreated(this, key);
        }

        public bool IsValidKey(string key)
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

            if (StringValueChanged != null)
                StringValueChanged(this, new StringValueChangedEventArgs(key, value));
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
}
