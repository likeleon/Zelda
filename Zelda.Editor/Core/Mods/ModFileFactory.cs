using System;
using Zelda.Editor.Core.Mods.ModFiles;
using Zelda.Game;

namespace Zelda.Editor.Core.Mods
{
    class ModFileFactory
    {
        public static IModFile Create(IMod mod, string path)
        {
            if (mod == null)
                throw new ArgumentNullException("mod");

            if (path.IsNullOrEmpty())
                throw new ArgumentNullException("path");

            var fileType = GetFileType(mod, path);
            switch (fileType)
            {
                case ModFileType.RootDirectory:
                    return new ModRootDirectory(mod.Name, path);

                default:
                    throw new NotImplementedException();
            }
        }

        static ModFileType GetFileType(IMod mod, string path)
        {
            if (path == mod.RootPath)
                return ModFileType.RootDirectory;
            else
                throw new NotImplementedException();
        }
    }
}
