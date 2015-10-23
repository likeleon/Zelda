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
        readonly IMod _mod;
        readonly string _languageId;
        readonly StringResources _resources = new StringResources();
        readonly StringResources _translationResources = new StringResources();
        readonly NodeTree _stringTree = new NodeTree(".");

        public Node Root { get { return _stringTree.Root; } }

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
            _stringTree.Clear();
            _resources.Strings.Keys.Do(id => _stringTree.AddKey(id));
            UpdateChildIcons(_stringTree.Root);
        }

        void UpdateChildIcons(Node node)
        {
            node.Children.Do(child => UpdateChildIcons(child));
            node.Icon = GetIcon(node);
        }

        Uri GetIcon(Node node)
        {
            if (!StringExists(node.Key))
            {
                if (TranslatedStringExists(node.Key))
                {
                    if (node.BindableChildren.Any())
                        return "icon_strings_missing.png".ToIconUri();
                    else
                        return "icon_string_missing.png".ToIconUri();
                }
                return "icon_folder_open.png".ToIconUri();
            }
            else
            {
                if (node.BindableChildren.Any())
                    return "icon_strings.png".ToIconUri();
                else
                    return "icon_string.png".ToIconUri();
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
    }
}
