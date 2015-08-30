using System;
using System.Collections.Generic;
using System.Linq;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    class ModFileContextMenuBuilder
    {
        const ContextMenuItem Separator = null;

        readonly List<ContextMenuItem> _items = new List<ContextMenuItem>();
        readonly IMod _mod;
        readonly string _path;

        public ModFileContextMenuBuilder(IMod mod, string path)
        {
            if (mod == null)
                throw new ArgumentNullException("mod");
            if (path == null)
                throw new ArgumentNullException("path");

            _mod = mod;
            _path = path;
        }

        public List<ContextMenuItem> Build()
        {
            BuildNewMenus();
            return _items;
        }

        void BuildNewMenus()
        {
            var resources = _mod.Resources;

            var resourceType = ResourceType.Map;
            var elementId = "";
            var isPotentialResourceElement = _mod.IsPotentialResourceElement(_path, ref resourceType, ref elementId);
            var isDeclaredResourceElement = isPotentialResourceElement && resources.Exists(resourceType, elementId);
            var isDir = _mod.IsDirectory(_path);

            ContextMenuItem newResourceElementMenuItem = null;

            if (isPotentialResourceElement)
            {
                if (isDeclaredResourceElement)
                    return;

                var resourceTypeFriendlyName = resources.GetFriendlyName(resourceType);
                var resourceTypeName = resources.GetTypeName(resourceType);

                newResourceElementMenuItem = new ContextMenuItem()
                {
                    Text = "Add to mod as {0}".F(resourceTypeFriendlyName),
                    IconSource = "/Resources/Icons/icon_resource_{0}.png".F(resourceTypeName).ToIconUri(),
                    Command = new RelayCommand(OnNewResourceElementExecute),
                    CommandParameter = _path
                };
            }
            else if (_mod.IsResourceDirectory(_path, ref resourceType) ||
                     (isDir && _mod.IsInResourceDirectory(_path, ref resourceType)))
            {
                var resourceTypeCreateFriendlyName = resources.GetCreateFriendlyName(resourceType);
                var resourceTypeName = resources.GetTypeName(resourceType);

                newResourceElementMenuItem = new ContextMenuItem()
                {
                    Text = resourceTypeCreateFriendlyName,
                    IconSource = "/Resources/Icons/icon_resource_{0}.png".F(resourceTypeName).ToIconUri(),
                    Command = new RelayCommand(OnNewResourceElementExecute),
                    CommandParameter = _path
                };
            }

            if (newResourceElementMenuItem != null)
            {
                _items.Add(newResourceElementMenuItem);
                _items.Add(Separator);
            }

            if (isDir)
            {
                _items.Add(new ContextMenuItem()
                {
                    Text = "New folder...",
                    IconSource = "/Resources/Icons/icon_folder_closed.png".ToIconUri(),
                    Command = new RelayCommand(OnNewDirectoryExecute),
                    CommandParameter = _path
                });
            }
        }

        static void OnNewResourceElementExecute(object param)
        {
        }

        static void OnNewDirectoryExecute(object param)
        {
        }
    }

    static class ModFileContextMenuBuilderExtensions
    {
        public static IEnumerable<ContextMenuItem> AddSeparatorIfNotEmpty(this IEnumerable<ContextMenuItem> e)
        {
            if (e.Any())
            {
                const ContextMenuItem separator = null;
                return e.Concat(separator.Yield());
            }
            else
                return e;
        }
    }
}
