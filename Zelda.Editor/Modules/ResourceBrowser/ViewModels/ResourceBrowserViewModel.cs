using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Windows;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Controls.ViewModels;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.Mods.Models;
using Zelda.Editor.Modules.Mods.Services;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser.ViewModels
{
    [Export(typeof(IResourceBrowser))]
    class ResourceBrowserViewModel : Tool, IResourceBrowser
    {
        readonly IModService _modService;
        readonly IShell _shell;
        IModFile _selectedModFile;

        public override PaneLocation PreferredLocation { get { return PaneLocation.Left; } }
        public override double PreferredWidth { get { return 400.0; } }
        public IEnumerable<IModFile> ModRootFiles { get; private set; }
        public IModFile SelectedModFile
        {
            get { return _selectedModFile; }
            set { this.SetProperty(ref _selectedModFile, value); }
        }
        public IMod Mod { get { return _modService.Mod; } }

        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand RenameCommand { get; private set; }

        [ImportingConstructor]
        public ResourceBrowserViewModel(IModService modService, IShell shell)
        {
            _modService = modService;
            _shell = shell;

            DisplayName = "Resource Browser";
            DeleteCommand = new RelayCommand(_ => Delete(SelectedModFile), _ => SelectedModFile != null);
            RenameCommand = new RelayCommand(_ => Rename(SelectedModFile), _ => SelectedModFile != null);

            _modService.Loaded += ModService_Loaded;
            _modService.Unloaded += ModService_Unloaded;
        }

        void ModService_Loaded(object sender, IMod loadedMod)
        {
            ModRootFiles = ModFileBuilder.Build(loadedMod).Yield();
            NotifyOfPropertyChange(() => ModRootFiles);
            ModRootFiles.Where(f => f.Children.Any()).Do(f => f.IsExpanded = true);

            loadedMod.FileRenamed += LoadedMod_FileRenamed;
            loadedMod.Resources.ElementDescriptionChanged += Resources_ElementDescriptionChanged;

            OpenDialogsEditor("en");
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

        public void NewResourceElement(IModFile parent)
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
                dynamic settings = new ExpandoObject();
                settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (IoC.Get<IWindowManager>().ShowDialog(dialog, null, settings) != true)
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

        public void NewDirectory(IModFile parent)
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

        public void Delete(IModFile file)
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
                    var answer = MessageBox.Show(question, "Delete confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (answer != MessageBoxResult.Yes)
                        return;

                    mod.DeleteResourceElement(resourceType, elementId);
                    file.RemoveFromParent();
                }
                else
                {
                    if (Directory.Exists(path))
                    {
                        if (Directory.EnumerateFileSystemEntries(path).Any())
                        {
                            "Folder is not empty".ShowWarningDialog();
                        }
                        else
                        {
                            var question = "Do you really want to delete folder '{0}'?".F(pathFromRoot);
                            var answer = MessageBox.Show(question, "Delete confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (answer != MessageBoxResult.Yes)
                                return;

                            mod.DeleteDirectory(path);
                            file.RemoveFromParent();
                        }
                    }
                    else
                    {
                        var question = "Do you really want to delete file '{0}'?".F(pathFromRoot);
                        var answer = MessageBox.Show(question, "Delete confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (answer != MessageBoxResult.Yes)
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

        public void Rename(IModFile file)
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

        public void ChangeDescription(IModFile file)
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

        public void Open(string path)
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
                OpenStringsEditor(absolutePath, elementId);
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

        void OpenStringsEditor(string absolutePath, string elementId)
        {
            throw new NotImplementedException();
        }

        void OpenDialogsEditor(string languageId)
        {
            var mod = _modService.Mod;
            var path = mod.GetDialogsPath(languageId);
            if (!mod.IsInRootPath(path))
                return;

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

            editor = provider.Create();

            var viewAware = (IViewAware)editor;
            viewAware.ViewAttached += (sender, e) =>
            {
                var frameworkElement = (FrameworkElement)e.View;
                frameworkElement.Loaded += async (sender2, e2) => await provider.Open(editor, path);
            };

            return editor;
        }
    }
}
