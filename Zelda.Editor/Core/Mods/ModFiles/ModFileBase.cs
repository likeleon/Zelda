using Caliburn.Micro;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Zelda.Editor.Core.Mods.ModFiles
{
    abstract class ModFileBase : PropertyChangedBase, IModFile
    {
        readonly List<IModFile> _children = new List<IModFile>();

        public ModFileType FileType { get; private set; }
        public string Path { get; private set; }

        public IModFile Parent { get; private set; }
        public IEnumerable<IModFile> Children { get { return _children; } }

        public abstract string Name { get; }
        public abstract Uri Icon { get; }
        public virtual string ToolTip { get { return string.Empty; } }

        public int Depth
        {
            get
            {
                var max = Children.Select(c => c.Depth).DefaultIfEmpty().Max();
                return max + 1;
            }
        }

        public int Level
        {
            get
            {
                if (Parent == null)
                    return Depth;
                else
                    return Parent.Level - 1;
            }
        }

        protected ModFileBase(ModFileType fileType, string path, IModFile parent)
        {
            FileType = fileType;
            Path = path;
            Parent = parent;
        }

        public void AddChild(IModFile child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            _children.Add(child);

            NotifyOfPropertyChange("Children");
        }
    }
}
