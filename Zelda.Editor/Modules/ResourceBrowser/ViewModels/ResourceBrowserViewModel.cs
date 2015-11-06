using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.Controls.ViewModels;
using Zelda.Editor.Core.Menus;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Core.Threading;
using Zelda.Editor.Modules.ContextMenus;
using Zelda.Editor.Modules.ContextMenus.Models;
using Zelda.Editor.Modules.Mods.Models;
using Zelda.Editor.Modules.Mods.Services;
using Zelda.Editor.Modules.ResourceBrowser.Commands;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser.ViewModels
{
    [Export(typeof(IResourceBrowser))]
    class ResourceBrowserViewModel : Tool, IResourceBrowser,
        ICommandHandler<OpenResourceCommandDefinition>,
        ICommandHandler<OpenMapScriptCommandDefinition>,
        ICommandHandler<OpenLanguageStringsCommandDefinition>,
        ICommandHandler<NewResourceElementCommandDefinition>,
        ICommandHandler<NewDirectoryCommandDefinition>,
        ICommandHandler<RenameCommandDefinition>,
        ICommandHandler<ChangeDescriptionCommandDefinition>,
        ICommandHandler<DeleteCommandDefinition>
    {
        readonly IModService _modService;
        readonly IShell _shell;
        readonly ContextMenuModel _contextMenu = new ContextMenuModel();
        IModFile _selectedModFile;

        public override PaneLocation PreferredLocation { get { return PaneLocation.Left; } }
        public override double PreferredWidth { get { return 400.0; } }
        public IEnumerable<IModFile> ModRootFiles { get; private set; }
        public IMod Mod { get { return _modService.Mod; } }
        public IContextMenu ContextMenu { get { return _contextMenu; } }
        public IModFile SelectedModFile
        {
            get { return _selectedModFile; }
            set { this.SetProperty(ref _selectedModFile, value); }
        }

        [ImportingConstructor]
        public ResourceBrowserViewModel(IModService modService, IShell shell)
        {
            _modService = modService;
            _shell = shell;

            DisplayName = "Resource Browser";

            _modService.Loaded += ModService_Loaded;
            _modService.Unloaded += ModService_Unloaded;
        }

        public bool BuildContextMenu()
        {
            _contextMenu.Clear();

            if (SelectedModFile == null)
                return false;

            var menuDefinitions = CreateContextMenuItems();
            IoC.Get<IContextMenuBuilder>().BuildContextMenu(menuDefinitions, _contextMenu);
            return _contextMenu.Count > 0;
        }

        List<MenuItemDefinition> CreateContextMenuItems()
        {
            var menuItems = new List<MenuItemDefinition>();
            menuItems.AddRange(CreateOpenMenuGroupItems());
            menuItems.AddRange(CreateNewMenuGroupItems());
            menuItems.AddRange(CreateRenameMenuGroupItems());
            menuItems.AddRange(CreateDeleteMenuGroupItems());
            return menuItems;
        }

        IEnumerable<MenuItemDefinition> CreateOpenMenuGroupItems()
        {
            var openAction = new OpenResourceCommandDefinition();

            var path = SelectedModFile.Path;
            var resourceType = ResourceType.Map;
            var elementId = "";
            if (Mod.IsResourceElement(path, ref resourceType, ref elementId))
            {
                var resourceTypeName = Mod.Resources.GetTypeName(resourceType);
                openAction.SetIconSource("icon_resource_{0}.png".F(resourceTypeName).ToIconUri());

                switch (resourceType)
                {
                    case ResourceType.Map:
                        {
                            yield return new CommandMenuItemDefinition<OpenMapScriptCommandDefinition>(
                                new OpenMapScriptCommandDefinition(), ContextMenuDefinitions.OpenMenuGroup, 1);
                        }
                        break;

                    case ResourceType.Language:
                        openAction.SetText("Open Dialogs");
                        {
                            var action = new OpenLanguageStringsCommandDefinition();
                            action.SetIconSource(openAction.IconSource);
                            yield return new CommandMenuItemDefinition<OpenLanguageStringsCommandDefinition>(
                                action, ContextMenuDefinitions.OpenMenuGroup, 1);
                        }
                        break;

                    case ResourceType.Tileset:
                    case ResourceType.Sprite:
                    case ResourceType.Item:
                    case ResourceType.Enemy:
                    case ResourceType.Entity:
                        break;

                    case ResourceType.Music:
                    case ResourceType.Sound:
                    case ResourceType.Font:
                        openAction = null;
                        break;
                }
            }
            else if (Mod.IsScript(path))
            {
                openAction.SetIconSource("icon_script.png".ToIconUri());
            }
            else if (Mod.IsModRootDirectory(path))
            {
                openAction.SetText("Open properties");
            }
            else
            {
                openAction = null;
            }

            if (openAction != null)
                yield return new CommandMenuItemDefinition<OpenResourceCommandDefinition>(
                    openAction, ContextMenuDefinitions.OpenMenuGroup, 0);
        }

        IEnumerable<MenuItemDefinition> CreateNewMenuGroupItems()
        {
            var path = SelectedModFile.Path;
            var resourceType = ResourceType.Map;
            var elementId = "";
            var isPotentialResourceElement = Mod.IsPotentialResourceElement(path, ref resourceType, ref elementId);
            var isDeclaredResourceElement = isPotentialResourceElement && Mod.Resources.Exists(resourceType, elementId);
            var isDir = Mod.IsDirectory(path);

            NewResourceElementCommandDefinition newResourceCommandDefinition = null;

            if (isPotentialResourceElement)
            {
                if (isDeclaredResourceElement)
                    yield break;

                var resourceTypeFriendlyName = Mod.Resources.GetFriendlyName(resourceType);
                var resourceTypeName = Mod.Resources.GetTypeName(resourceType);

                newResourceCommandDefinition = new NewResourceElementCommandDefinition();
                newResourceCommandDefinition.SetText("Add to mod as {0}".F(resourceTypeFriendlyName));
                newResourceCommandDefinition.SetIconSource("icon_resource_{0}.png".F(resourceTypeName).ToIconUri());
            }
            else if (Mod.IsResourceDirectory(path, ref resourceType) ||
                     (isDir && Mod.IsInResourceDirectory(path, ref resourceType)))
            {
                var resourceTypeCreateFriendlyName = Mod.Resources.GetCreateFriendlyName(resourceType);
                var resourceTypeName = Mod.Resources.GetTypeName(resourceType);

                newResourceCommandDefinition = new NewResourceElementCommandDefinition();
                newResourceCommandDefinition.SetText(resourceTypeCreateFriendlyName);
                newResourceCommandDefinition.SetIconSource("icon_resource_{0}.png".F(resourceTypeName).ToIconUri());
            }

            if (newResourceCommandDefinition != null)
                yield return new CommandMenuItemDefinition<NewResourceElementCommandDefinition>(
                    newResourceCommandDefinition, ContextMenuDefinitions.NewMenuGroup, 0);

            if (isDir)
                yield return new CommandMenuItemDefinition<NewDirectoryCommandDefinition>(ContextMenuDefinitions.NewMenuGroup, 1);
        }

        IEnumerable<MenuItemDefinition> CreateRenameMenuGroupItems()
        {
            var path = SelectedModFile.Path;
            if (path == Mod.RootPath)
                yield break;

            var resourceType = ResourceType.Map;
            if (Mod.IsResourceDirectory(path, ref resourceType))
                yield break;

            yield return new CommandMenuItemDefinition<RenameCommandDefinition>(ContextMenuDefinitions.RenameMenuGroup, 0);

            var elementId = "";
            if (Mod.IsResourceElement(path, ref resourceType, ref elementId))
                yield return new CommandMenuItemDefinition<ChangeDescriptionCommandDefinition>(ContextMenuDefinitions.RenameMenuGroup, 0);
        }

        IEnumerable<MenuItemDefinition> CreateDeleteMenuGroupItems()
        {
            var path = SelectedModFile.Path;
            if (path == Mod.RootPath)
                yield break;

            var resourceType = ResourceType.Map;
            if (Mod.IsResourceDirectory(path, ref resourceType))
                yield break;

            yield return new CommandMenuItemDefinition<DeleteCommandDefinition>(ContextMenuDefinitions.DeleteMenuGroup, 0);
        }

            void ModService_Loaded(object sender, IMod loadedMod)
        {
            ModRootFiles = ModFileBuilder.Build(loadedMod).Yield();
            NotifyOfPropertyChange(() => ModRootFiles);
            ModRootFiles.Where(f => f.Children.Any()).Do(f => f.IsExpanded = true);

            loadedMod.FileRenamed += LoadedMod_FileRenamed;
            loadedMod.Resources.ElementDescriptionChanged += Resources_ElementDescriptionChanged;

            OpenStringsEditor("en");
        }


        void LoadedMod_FileRenamed(object sender, FileRenamedEventArgs e)
        {
            var file = FindFile(e.OldPath);
            if (file != null)
                file.Path = e.NewPath;
        }

        static IEnumerable<IModFile> GetDescendants(IModFile file)
        {
            return file.Yield().Concat(file.Children.SelectMany(c => GetDescendants(c)));
        }

        IModFile FindFile(string path)
        {
            return GetDescendants(ModRootFiles.Single()).FirstOrDefault(f => f.Path == path);
        }

        void Resources_ElementDescriptionChanged(object sender, ElementDescriptionChangedEventArgs e)
        {
            var path = Mod.GetResourceElementPath(e.ResourceType, e.Id);
            var file = FindFile(path);
            if (file != null)
                file.Description = e.Description;
        }

        void ModService_Unloaded(object sender, IMod unloadedMod)
        {
            unloadedMod.Resources.ElementDescriptionChanged -= Resources_ElementDescriptionChanged;
            unloadedMod.FileRenamed -= LoadedMod_FileRenamed;
            ModRootFiles = null;
            NotifyOfPropertyChange(() => ModRootFiles);
        }

        void NewResourceElement(IModFile parent)
        {
            try
            {
                var path = parent.Path;
                var mod = _modService.Mod;
                var resources = mod.Resources;
                var resourceType = ResourceType.Map;
                var initialIdValue = "";

                if (mod.IsPotentialResourceElement(path, ref resourceType, ref initialIdValue))
                {
                    if (resources.Exists(resourceType, initialIdValue))
                        return;
                }
                else
                {
                    if (!mod.IsResourceDirectory(path, ref resourceType) &&
                        !mod.IsInResourceDirectory(path, ref resourceType))
                        return;

                    var resourceDir = mod.GetResourcePath(resourceType);
                    if (path != resourceDir)
                        initialIdValue = path.Substring(resourceDir.Length + 1) + Path.DirectorySeparatorChar;
                    else
                        initialIdValue = "";
                }

                var resourceTypeName = mod.Resources.GetFriendlyName(resourceType);
                var dialog = new NewResourceElementViewModel(resourceTypeName, mod) { Id = initialIdValue };
                if (dialog.ShowDialog() != true)
                    return;

                var elementId = dialog.Id;
                var description = dialog.Description;

                mod.CreateResourceElement(resourceType, elementId, description);

                var createdPath = mod.GetResourceElementPath(resourceType, elementId);
                if (mod.Exists(createdPath))
                {
                    var isReplace = (createdPath == path);
                    var file = ModFileBuilder.Create(mod, createdPath, isReplace ? parent.Parent : parent);
                    if (isReplace)
                        parent.RemoveFromParent();

                    SelectedModFile = file;
                    // TODO: Open file
                }
            }
            catch (Exception e)
            {
                e.ShowDialog();
            }
        }

        void NewDirectory(IModFile parent)
        {
            try
            {
                var dirName = TextInputViewModel.GetText("New folder", "Folder name:", "");
                if (dirName == null)
                    return;

                var mod = _modService.Mod;
                mod.CheckValidFileName(dirName);
                mod.CreateDirectory(parent.Path, dirName);

                var dirPath = Path.Combine(parent.Path, dirName);
                SelectedModFile = ModFileBuilder.Create(mod, dirPath, parent);
            }
            catch (Exception ex)
            {
                ex.ShowDialog();
            }
        }

        void Delete(IModFile file)
        {
            var path = file.Path;
            var mod = _modService.Mod;
            if (path == mod.RootPath)
                return;

            var resourceType = ResourceType.Map;
            if (mod.IsResourceDirectory(path, ref resourceType))
                return;

            try
            {
                var pathFromRoot = path.Substring(0, mod.RootPath.Length);
                var elementId = "";
                if (mod.IsResourceElement(path, ref resourceType, ref elementId))
                {
                    var resources = mod.Resources;
                    var resourceFriendlyName = resources.GetFriendlyName(resourceType);
                    var question = "Do you really want to delete {0} '{1}'?".F(resourceFriendlyName, elementId);
                    if (!question.AskYesNo("Delete confirmation"))
                        return;

                    mod.DeleteResourceElement(resourceType, elementId);
                    file.RemoveFromParent();
                }
                else
                {
                    if (Directory.Exists(path))
                    {
                        if (Directory.EnumerateFileSystemEntries(path).Any())
                            "Folder is not empty".ShowWarningDialog();
                        else
                        {
                            var question = "Do you really want to delete folder '{0}'?".F(pathFromRoot);
                            if (!question.AskYesNo("Delete confirmation"))
                                return;

                            mod.DeleteDirectory(path);
                            file.RemoveFromParent();
                        }
                    }
                    else
                    {
                        var question = "Do you really want to delete file '{0}'?".F(pathFromRoot);
                        if (!question.AskYesNo("Delete confirmation"))
                            return;

                        mod.DeleteFile(path);
                        file.RemoveFromParent();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ShowDialog();
            }
        }

        void Rename(IModFile file)
        {
            var path = file.Path;
            var mod = _modService.Mod;
            if (path == mod.RootPath)
                return;

            // TODO: Warn if unsaved

            var resourceType = ResourceType.Map;
            if (mod.IsResourceDirectory(path, ref resourceType))
                return;

            try
            {
                var resources = mod.Resources;
                var elementId = "";
                if (mod.IsResourceElement(path, ref resourceType, ref elementId))
                {
                    var resourceFriendlyName = resources.GetFriendlyName(resourceType);
                    var label = "New id for {0} '{1}'".F(resourceFriendlyName, elementId);
                    var newId = TextInputViewModel.GetText("Rename resource", label, elementId);
                    if (newId == null)
                        return;

                    if (newId != elementId)
                    {
                        mod.CheckValidFileName(newId);
                        mod.RenameResourceElement(resourceType, elementId, newId);
                    }
                }
                else
                {
                    var fileName = Path.GetFileName(path);
                    var label = "New name for file '{0}'".F(fileName);
                    var newFileName = TextInputViewModel.GetText("Rename file", label, fileName);
                    if (newFileName == null)
                        return;

                    if (newFileName != fileName)
                    {
                        mod.CheckValidFileName(fileName);
                        var newPath = Path.Combine(Path.GetDirectoryName(path), newFileName);
                        mod.RenameFile(path, newPath);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ShowDialog();
            }
        }

        void ChangeDescription(IModFile file)
        {
            var path = file.Path;
            var mod = _modService.Mod;
            var resourceType = ResourceType.Map;
            var elementId = "";
            if (!mod.IsResourceElement(path, ref resourceType, ref elementId))
                return;

            var resources = mod.Resources;
            var resourceFriendlyTypeName = resources.GetFriendlyName(resourceType);
            var oldDescription = resources.GetDescription(resourceType, elementId);
            var label = "New description for {0} '{1}':".F(resourceFriendlyTypeName, elementId);
            var newDescription = TextInputViewModel.GetText("Change description", label, oldDescription);
            if (newDescription == null)
                return;

            try
            {
                if (newDescription.Length <= 0)
                    throw new Exception("Empty description");

                resources.SetDescription(resourceType, elementId, newDescription);
                resources.Save();

                file.Description = newDescription;
            }
            catch (Exception ex)
            {
                ex.ShowDialog();
            }
        }

        void Open(string path)
        {
            var absolutePath = Path.GetFullPath(path);
            var mod = _modService.Mod;
            if (!mod.IsInRootPath(absolutePath))
                return;

            var resourceType = ResourceType.Map;
            var elementId = "";
            if (mod.IsResourceElement(absolutePath, ref resourceType, ref elementId))
                OpenResource(resourceType, elementId);
            else if (mod.IsDialogsFile(absolutePath, ref elementId))
                OpenDialogsEditor(elementId);
            else if (mod.IsStringsFile(absolutePath, ref elementId))
                OpenStringsEditor(elementId);
            else if (mod.IsScript(absolutePath))
                OpenTextEditor(absolutePath);
            else if (mod.IsModRootDirectory(absolutePath))
                OpenModPropertiesEditor();
        }

        void OpenResource(ResourceType resourceType, string id)
        {
            switch (resourceType)
            {
                case ResourceType.Language:
                    OpenDialogsEditor(id);
                    break;

                case ResourceType.Music:
                case ResourceType.Sound:
                case ResourceType.Font:
                    break;
            }
        }

        void OpenModPropertiesEditor()
        {
            throw new NotImplementedException();
        }

        void OpenTextEditor(string absolutePath)
        {
            throw new NotImplementedException();
        }

        void OpenStringsEditor(string languageId)
        {
            var mod = _modService.Mod;
            var path = mod.GetStringsPath(languageId);
            if (mod.IsInRootPath(path))
                _shell.OpenDocument(GetEditor(path));
        }

        void OpenDialogsEditor(string languageId)
        {
            var mod = _modService.Mod;
            var path = mod.GetDialogsPath(languageId);
            if (mod.IsInRootPath(path))
                _shell.OpenDocument(GetEditor(path));
        }

        IDocument GetEditor(string path)
        {
            var provider = IoC.GetAllInstances(typeof(IEditorProvider))
                .Cast<IEditorProvider>()
                .FirstOrDefault(p => p.Handles(path));
            if (provider == null)
                return null;

            var editor = provider.Find(_shell.Documents, path);
            if (editor != null)
                return editor;

            return provider.Open(path);
        }

        public void OpenSelectedFile()
        {
            if (SelectedModFile != null)
                Open(SelectedModFile.Path);
        }

        public void Open(IModFile modFile)
        {
            if (modFile == null)
                throw new ArgumentNullException("modFile");

            Open(modFile.Path);
        }

        public void RenameSelectedFile()
        {
            if (SelectedModFile != null)
                Rename(SelectedModFile);
        }

        public void DeleteSelectedFile()
        {
            if (SelectedModFile != null)
                Delete(SelectedModFile);
        }

        #region Command Handlings
        void ICommandHandler<OpenResourceCommandDefinition>.Update(Command command)
        {
            command.Enabled = (SelectedModFile != null);
        }

        Task ICommandHandler<OpenResourceCommandDefinition>.Run(Command command)
        {
            Open(SelectedModFile.Path);
            return TaskUtility.Completed;
        }

        void ICommandHandler<OpenMapScriptCommandDefinition>.Update(Command command)
        {
            command.Enabled = (SelectedModFile != null);
        }

        Task ICommandHandler<OpenMapScriptCommandDefinition>.Run(Command command)
        {
            Open(Mod.GetMapScriptPath(SelectedModFile.Path));
            return TaskUtility.Completed;
        }

        void ICommandHandler<OpenLanguageStringsCommandDefinition>.Update(Command command)
        {
            command.Enabled = (SelectedModFile != null);
        }

        Task ICommandHandler<OpenLanguageStringsCommandDefinition>.Run(Command command)
        {
            Open(Mod.GetStringsPath(SelectedModFile.Path));
            return TaskUtility.Completed;
        }

        void ICommandHandler<NewResourceElementCommandDefinition>.Update(Command command)
        {
            command.Enabled = (SelectedModFile != null);
        }

        Task ICommandHandler<NewResourceElementCommandDefinition>.Run(Command command)
        {
            NewResourceElement(SelectedModFile);
            return TaskUtility.Completed;
        }

        void ICommandHandler<NewDirectoryCommandDefinition>.Update(Command command)
        {
            command.Enabled = (SelectedModFile != null);
        }

        Task ICommandHandler<NewDirectoryCommandDefinition>.Run(Command command)
        {
            NewDirectory(SelectedModFile);
            return TaskUtility.Completed;
        }

        void ICommandHandler<RenameCommandDefinition>.Update(Command command)
        {
            command.Enabled = (SelectedModFile != null);
        }

        Task ICommandHandler<RenameCommandDefinition>.Run(Command command)
        {
            Rename(SelectedModFile);
            return TaskUtility.Completed;
        }

        void ICommandHandler<ChangeDescriptionCommandDefinition>.Update(Command command)
        {
            command.Enabled = (SelectedModFile != null);
        }

        Task ICommandHandler<ChangeDescriptionCommandDefinition>.Run(Command command)
        {
            ChangeDescription(SelectedModFile);
            return TaskUtility.Completed;
        }

        void ICommandHandler<DeleteCommandDefinition>.Update(Command command)
        {
            command.Enabled = (SelectedModFile != null);
        }

        Task ICommandHandler<DeleteCommandDefinition>.Run(Command command)
        {
            Delete(SelectedModFile);
            return TaskUtility.Completed;
        }
        #endregion
    }
}
