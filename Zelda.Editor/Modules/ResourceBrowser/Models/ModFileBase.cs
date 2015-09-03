﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    abstract class ModFileBase : PropertyChangedBase, IModFile
    {
        readonly ObservableCollection<IModFile> _children = new ObservableCollection<IModFile>();
        string _description;

        public abstract ModFileType FileType { get; }
        public string Path { get; set; }

        public IModFile Parent { get; set; }
        public IEnumerable<IModFile> Children { get { return _children; } }

        public virtual string Name { get { return System.IO.Path.GetFileName(Path); } }
        public abstract Uri Icon { get; }
        public virtual string Description { get { return string.Empty; } }
        public string Type { get; set; }
        public string ToolTip { get; set; }

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

        public void AddChild(IModFile child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            _children.Add(child);
        }
    }
}