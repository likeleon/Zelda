using System;
using System.Collections.Generic;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    class ModFileContextMenuBuilder
    {
        public static IEnumerable<ContextMenuItem> Build(IMod mod, string path)
        {
            foreach (var menuItem in BuildOpenMenu(mod, path))
                yield return menuItem;
        }

        static IEnumerable<ContextMenuItem> BuildOpenMenu(IMod mod, string path)
        {
            var resources = mod.Resources;

            var resourceType = ResourceType.Map;
            var elementId = "";
            var isPotentialResourceElement = mod.IsPotentialResourceElement(path, ref resourceType, ref elementId);
            var isDeclaredResourceElement = isPotentialResourceElement && resources.Exists(resourceType, elementId);
            var isDir = mod.IsDirectory(path);

            ContextMenuItem newResourceElementMenuItem = null;

            if (isPotentialResourceElement)
            {
                if (isDeclaredResourceElement)
                    yield break;

                var resourceTypeFriendlyName = resources.GetFriendlyName(resourceType);
                var resourceTypeName = resources.GetTypeName(resourceType);

                newResourceElementMenuItem = new ContextMenuItem()
                {
                    Text = "Add to mod as {0}".F(resourceTypeFriendlyName),
                    IconSource = "Resources/Icons/icon_resource_{0}.png".F(resourceTypeName).ToIconUri(),
                    Command = new RelayCommand(OnNewResourceElementExecute),
                    CommandParameter = path
                };
            }
            else if (mod.IsResourceDirectory(path, ref resourceType) ||
                     (isDir && mod.IsInResourceDirectory(path, ref resourceType)))
            {
                var resourceTypeCreateFriendlyName = resources.GetCreateFriendlyName(resourceType);
                var resourceTypeName = resources.GetTypeName(resourceType);

                newResourceElementMenuItem = new ContextMenuItem()
                {
                    Text = resourceTypeCreateFriendlyName,
                    IconSource = "/Resources/Icons/icon_resource_{0}.png".F(resourceTypeName).ToIconUri(),
                    Command = new RelayCommand(OnNewResourceElementExecute),
                    CommandParameter = path
                };
            }

            if (newResourceElementMenuItem != null)
                yield return newResourceElementMenuItem;

            if (isDir)
            {
                yield return new ContextMenuItem()
                {
                    Text = "New folder...",
                    IconSource = "/Resources/Icons/icon_folder_closed.png".ToIconUri(),
                    Command = new RelayCommand(OnNewDirectoryExecute),
                    CommandParameter = path
                };
            }
        }

        static void OnNewResourceElementExecute(object param)
        {
        }

        static void OnNewDirectoryExecute(object param)
        {
        }
    }
}
