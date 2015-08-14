using System;
using System.IO;
using System.Linq;
using Zelda.Editor.Core.Mods.ModFiles;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods
{
    class ModFileBuilder
    {
        readonly IMod _mod;

        public static IModFile Build(IMod mod)
        {
            var builder = new ModFileBuilder(mod);
            return builder.BuildRecursive(builder._mod.RootPath, null);
        }

        ModFileBuilder(IMod mod)
        {
            if (mod == null)
                throw new ArgumentNullException("mod");

            _mod = mod;
        }

        IModFile BuildRecursive(string dirPath, ModFileBase parent)
        {
            var directory = CreateModFile(dirPath, parent);

            var resourceType = ResourceType.Map;
            var elementId = "";
            if (_mod.IsResourceElement(dirPath, ref resourceType, ref elementId))
                return directory;

            foreach (var childDirPath in Directory.EnumerateDirectories(dirPath))
            {
                var childDir = BuildRecursive(childDirPath, directory);
                directory.AddChild(childDir);
            }

            var acceptableFilePaths = Directory.EnumerateFiles(dirPath).Where(path => IsAcceptableFile(path));
            foreach (var filePath in acceptableFilePaths)
            {
                var file = CreateModFile(filePath, parent);
                directory.AddChild(file);
            }

            return directory;
        }

        bool IsAcceptableFile(string path)
        {
            if (IsScriptFile(path))
                return !IsMapScriptFile(path);

            var resourceType = ResourceType.Map;
            var elementId = "";
            if (_mod.IsPotentialResourceElement(path, ref resourceType, ref elementId))
                return true;

            return false;
        }

        bool IsScriptFile(string path)
        {
            const string csharpExtension = ".cs";
            return Path.GetExtension(path).Equals(csharpExtension, StringComparison.CurrentCultureIgnoreCase);
        }

        bool IsMapScriptFile(string path)
        {
            if (!IsScriptFile(path))
                throw new InvalidOperationException("Should be script file: '{0}'".F(path));

            var filePathXml = Path.ChangeExtension(path, ".xml");
            var resourceType = ResourceType.Map;
            var elementId = "";
            return (_mod.IsResourceElement(filePathXml, ref resourceType, ref elementId) && resourceType == ResourceType.Map);
        }

        ModFileBase CreateModFile(string path, ModFileBase parent)
        {
            var resourceType = ResourceType.Map;
            var elementId = string.Empty;

            ModFileBase modFile = null;

            if (_mod.IsModRootDirectory(path))
                modFile = CreateRootDirectory();
            else if (_mod.IsResourceElement(path, ref resourceType, ref elementId))
                modFile = CreateResourceElement(resourceType, elementId);
            else if (_mod.IsDirectory(path))
            {
                if (_mod.IsResourceDirectory(path, ref resourceType))
                    modFile = CreateResourceDictionary(resourceType);
                else
                    modFile = CreateNormalDirectory();
            }
            else
                modFile = CreateUnknownFile();

            modFile.Path = path;
            modFile.Parent = parent;
            modFile.ToolTip = GetToolTip(path);

            return modFile;
        }

        RootDirectory CreateRootDirectory()
        {
            return new RootDirectory(_mod.Name);
        }

        ResourceElement CreateResourceElement(ResourceType resourceType, string elementId)
        {
            var description = _mod.Resources.GetDescription(resourceType, elementId);
            return new ResourceElement(resourceType, description);
        }

        ResourceDirectory CreateResourceDictionary(ResourceType resourceType)
        {
            return new ResourceDirectory(resourceType);
        }

        NormalDirectory CreateNormalDirectory()
        {
            return new NormalDirectory();
        }

        UnknownFile CreateUnknownFile()
        {
            return new UnknownFile();
        }

        string GetToolTip(string path)
        {
            var resourceType = ResourceType.Map;
            var elementId = string.Empty;

            if (!_mod.IsPotentialResourceElement(path, ref resourceType, ref elementId))
                return string.Empty;

            var fileName = Path.GetFileName(path);
            if (_mod.Resources.Exists(resourceType, elementId))
            {
                if (_mod.Exists(_mod.GetResourceElementPath(resourceType, elementId)))
                    return fileName;
                else
                    return "{0} (file not found)".F(fileName);
            }
            else
                return "{0} (not in the mod)".F(fileName);
        }
    }
}
