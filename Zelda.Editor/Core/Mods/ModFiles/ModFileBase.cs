using System;
using System.Collections.Generic;

namespace Zelda.Editor.Core.Mods.ModFiles
{
    abstract class ModFileBase : IModFile
    {
        readonly List<IModFile> _children;

        public ModFileType FileType { get; private set; }
        public string Path { get; private set; }

        public IModFile Parent { get; set; }
        public IEnumerable<IModFile> Children { get { return _children; } }

        public abstract string Name { get; }
        public abstract Uri Icon { get; }
        public virtual string ToolTip { get { return string.Empty; } }

        protected ModFileBase(ModFileType fileType, string path)
        {
            FileType = fileType;
            Path = path;
        }
    }
}
