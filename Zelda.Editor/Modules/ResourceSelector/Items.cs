using Caliburn.Micro;
using System;
using System.Collections.Generic;
using Zelda.Editor.Core;

namespace Zelda.Editor.Modules.ResourceSelector
{
    abstract class Item : PropertyChangedBase
    {
        readonly List<Item> _children = new List<Item>();

        public Item Parent { get; private set; }
        public IEnumerable<Item> Children { get { return _children; } }
        public int ChildrenCount { get { return _children.Count; } }

        public T GetChild<T>(int index) where T : Item
        {
            return _children[index] as T;
        }

        public void AppendChild(Item child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            child.Parent = this;
            _children.Add(child);
            NotifyOfPropertyChange(() => Children);
        }

        public void InsertChild(int index, Item child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            child.Parent = this;
            _children.Insert(index, child);
            NotifyOfPropertyChange(() => Children);
        }

        public void RemoveChild(Item child)
        {
            if (child.Parent != this)
                throw new ArgumentException("Parent mismatch");

            _children.Remove(child);
            NotifyOfPropertyChange(() => Children);
        }
    }

    class InvisibleRootItem : Item
    {
    }

    class DirectoryItem : Item
    {
        public string Name { get; private set; }

        public DirectoryItem(string name)
        {
            Name = name;
        }
    }

    class ElementItem : Item
    {
        string _id;
        string _description;

        public string Id
        {
            get { return _id; }
            set { this.SetProperty(ref _id, value); }
        }

        public string Description
        {
            get { return _description; }
            set { this.SetProperty(ref _description, value); }
        }
    }

    class SpecialValueItem : Item
    {
        public string Id { get; set; }
        public string Text { get; set; }
    }
}
