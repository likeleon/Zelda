using System;
using System.IO;
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

            foreach (var childDirPath in Directory.EnumerateDirectories(dirPath))
            {
                var childDir = BuildRecursive(childDirPath, directory);
                directory.AddChild(childDir);
            }

            foreach (var filePath in Directory.EnumerateFiles(dirPath))
            {
                var file = CreateModFile(filePath, parent);
                directory.AddChild(file);
            }

            return directory;
        }

        ModFileBase CreateModFile(string path, ModFileBase parent)
        {
            ResourceType resourceType = ResourceType.Map;
            string elementId = string.Empty;

            if (_mod.IsModRootDirectory(path))
                return new RootDirectory(_mod.Name, path, parent);
            if (_mod.IsResourceElement(path, ref resourceType, ref elementId))
                return new ResourceElement(resourceType, path, parent);
            else if (_mod.IsDirectory(path))
            {                
                if (_mod.IsResourceDirectory(path, ref resourceType))
                    return new ResourceDirectory(resourceType, path, parent);
                else
                    return new NormalDirectory(path, parent);
            }
            else
                return new UnknownFile(path, parent);
        }
    }
}
