using System;
using System.IO;
using Zelda.Editor.Core.Mods.ModFiles;

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
            var fileType = GetFileType(dirPath);
            if (fileType != ModFileType.RootDirectory && parent == null)
                throw new ArgumentNullException("parent");

            var file = Create(dirPath, parent, fileType);
            foreach (var subDirPath in Directory.EnumerateDirectories(dirPath))
            {
                var subFile = BuildRecursive(subDirPath, file);
                file.AddChild(subFile);
            }

            return file;
        }

        ModFileBase Create(string path, ModFileBase parent, ModFileType fileType)
        {
            switch (fileType)
            {
                case ModFileType.RootDirectory:
                    return new RootDirectory(_mod.Name, path, parent);

                case ModFileType.NormalDirectory:
                    return new NormalDirectory(path, parent);

                default:
                    throw new NotImplementedException();
            }
        }

        ModFileType GetFileType(string path)
        {
            if (path == _mod.RootPath)
                return ModFileType.RootDirectory;
            else
                return ModFileType.NormalDirectory;
        }
    }
}
