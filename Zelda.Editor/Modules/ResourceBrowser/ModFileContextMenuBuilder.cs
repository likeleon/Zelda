using System;
using System.Collections.Generic;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Services;
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
            BuildOpenMenus();
            BuildNewMenus();
            BuildRenameMenus();
            BuildDeleteMenus();
            return _items;
        }

        void BuildOpenMenus()
        {
            var mod = _browser.Mod;
            var resources = mod.Resources;
            var path = _modFile.Path;

            var openMenuItem = new ContextMenuItem()
            {
                Text = "Open",
                Command = new RelayCommand(_ => _browser.Open(_modFile.Path))
            };

            var resourceType = ResourceType.Map;
            var elementId = "";
            if (mod.IsResourceElement(path, ref resourceType, ref elementId))
            {
                var resourceTypeName = resources.GetTypeName(resourceType);
                openMenuItem.IconSource = "/Resources/Icons/icon_resource_{0}.png".F(resourceTypeName).ToIconUri();

                switch (resourceType)
                {
                    case ResourceType.Map:
                        _items.Add(openMenuItem);
                        _items.Add(new ContextMenuItem()
                        {
                            Text = "Open Script",
                            IconSource = "/Resources/Icons/icon_script.png".ToIconUri(),
                            Command = new RelayCommand(_ => _browser.Open(mod.GetMapScriptPath(path)))
                        });
                        break;

                    case ResourceType.Language:
                        openMenuItem.Text = "Open Dialogs";
                        _items.Add(openMenuItem);
                        _items.Add(new ContextMenuItem()
                        {
                            Text = "Open Strings",
                            IconSource = openMenuItem.IconSource,
                            Command = new RelayCommand(_ => _browser.Open(mod.GetStringsPath(path)))
                        });
                        break;

                    case ResourceType.Tileset:
                    case ResourceType.Sprite:
                    case ResourceType.Item:
                    case ResourceType.Enemy:
                    case ResourceType.Entity:
                        _items.Add(openMenuItem);
                        break;

                    case ResourceType.Music:
                    case ResourceType.Sound:
                    case ResourceType.Font:
                        break;
                }
            }
            else if (mod.IsScript(path))
            {
                openMenuItem.IconSource = "/Resources/Icons/icon_script.png".ToIconUri();
                _items.Add(openMenuItem);
            }
            else if (mod.IsModRootDirectory(path))
            {
                openMenuItem.Text = "Open properties";
                _items.Add(openMenuItem);
            }
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
                    Command = new RelayCommand(_ => _browser.NewResourceElement(_modFile)),
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
                    Command = new RelayCommand(_ => _browser.NewResourceElement(_modFile))
                };
            }
            
            if (newResourceElementMenuItem != null)
            {
                if (_items.Count > 0)
                    _items.Add(Separator);

                _items.Add(newResourceElementMenuItem);
            }

            if (isDir)
            {
                if (_items.Count > 0 && newResourceElementMenuItem == null)
                    _items.Add(Separator);

                _items.Add(new ContextMenuItem()
                {
                    Text = "New folder...",
                    IconSource = "/Resources/Icons/icon_folder_closed.png".ToIconUri(),
                    Command = new RelayCommand(_ => _browser.NewDirectory(_modFile))
                });
            }
        }

        void BuildRenameMenus()
        {
            var mod = _browser.Mod;
            var path = _modFile.Path;
            if (path == mod.RootPath)
                return;

            var resourceType = ResourceType.Map;
            if (mod.IsResourceDirectory(path, ref resourceType))
                return;

            if (_items.Count > 0)
                _items.Add(Separator);

            _items.Add(new ContextMenuItem()
            {
                Text = "Rename...",
                IconSource = "/Resources/Icons/icon_rename.png".ToIconUri(),
                Command = new RelayCommand(_ => _browser.Rename(_modFile))
            });

            var elementId = "";
            if (mod.IsResourceElement(path, ref resourceType, ref elementId))
            {
                _items.Add(new ContextMenuItem()
                {
                    Text = "Change description...",
                    Command = new RelayCommand(_ => _browser.ChangeDescription(_modFile))
                });
            }
        }

        void BuildDeleteMenus()
        {
            var mod = _browser.Mod;
            var path = _modFile.Path;
            if (path == mod.RootPath)
                return;

            var resourceType = ResourceType.Map;
            if (mod.IsResourceDirectory(path, ref resourceType))
                return;

            if (_items.Count > 0)
                _items.Add(Separator);

            _items.Add(new ContextMenuItem()
            {
                Text = "Delete...",
                IconSource = "/Resources/Icons/icon_delete.png".ToIconUri(),
                Command = new RelayCommand(_ => _browser.Delete(_modFile))
            });
        }
    }
}
