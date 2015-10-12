using System;

namespace Zelda.Editor.Modules.Mods.Models
{
    class FileRenamedEventArgs : EventArgs
    {
        public string OldPath { get; private set; }
        public string NewPath { get; private set; }

        public FileRenamedEventArgs(string oldPath, string newPath)
        {
            OldPath = oldPath;
            NewPath = newPath;
        }
    }
}
