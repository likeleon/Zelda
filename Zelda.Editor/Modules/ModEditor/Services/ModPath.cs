using System;
using System.IO;
using Zelda.Game;

namespace Zelda.Editor.Modules.ModEditor.Services
{
    class ModPath
    {
        public string RootPath { get; private set; }
        public string PropertiesPath { get { return Path.Combine(RootPath, "mod.xml"); } }

        public ModPath(string rootPath)
        {
            if (rootPath.IsNullOrEmpty())
                throw new ArgumentException("rootPath should not be null and empty");

            RootPath = rootPath;
        }

        public bool Exists(string path)
        {
            return IsInRootPath(path) && File.Exists(path);
        }

        public bool IsInRootPath(string path)
        {
            return path.StartsWith(RootPath);
        }
    }
}
