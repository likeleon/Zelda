using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Windows;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Modules.ResourceBrowser.ViewModels;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    class ModFileContextMenuBuilder
    {
        const ContextMenuItem Separator = null;

        readonly List<ContextMenuItem> _items = new List<ContextMenuItem>();
        readonly ResourceBrowserViewModel _browser;
        readonly IModFile _modFile;

        public ModFileContextMenuBuilder(ResourceBrowserViewModel browser, IModFile modFile)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");
            if (modFile == null)
                throw new ArgumentNullException("modFile");

            _browser = browser;
            _modFile = modFile;
        }

        public List<ContextMenuItem> Build()
        {
            BuildNewMenus();
            return _items;
        }

        void BuildNewMenus()
        {
            var mod = _browser.Mod;
            var resources = mod.Resources;
            var path = _modFile.Path;

            var resourceType = ResourceType.Map;
            var elementId = "";
            var isPotentialResourceElement = mod.IsPotentialResourceElement(path, ref resourceType, ref elementId);
            var isDeclaredResourceElement = isPotentialResourceElement && resources.Exists(resourceType, elementId);
            var isDir = mod.IsDirectory(path);

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
                    Command = new RelayCommand(OnNewResourceElementExecute)
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
                    Command = new RelayCommand(OnNewDirectoryExecute)
                });
            }
        }

        void OnNewResourceElementExecute(object param)
        {
            _browser.NewResourceElement(_modFile);
        }

        static void OnNewDirectoryExecute(object param)
        {
        }
    }
}
