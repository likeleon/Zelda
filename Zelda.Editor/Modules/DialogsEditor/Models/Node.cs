using Caliburn.Micro;
using System;
using System.Collections.Generic;
using Zelda.Editor.Core;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.Models
{
    class Node : PropertyChangedBase
    {
        readonly Dictionary<string, Node> _children = new Dictionary<string, Node>(StringComparer.CurrentCulture);
        string _key = "";
        Node _parent;
        Uri _icon;

        public string Key
        {
            get { return _key; }
            set { this.SetProperty(ref _key, value); }
        }

        public NodeType Type { get; set; }

        public Uri Icon
        {
            get { return _icon; }
            set { this.SetProperty(ref _icon, value); }
        }

        public Node Parent
        {
            get { return _parent; }
            set { this.SetProperty(ref _parent, value); }
        }

        public int ChildrenCount { get { return _children.Count; } }

        public IEnumerable<Node> Children { get { return _children.Values; } }

        public BindableCollection<Node> BindableChildren { get; private set; }

        public Node()
        {
            BindableChildren = new BindableCollection<Node>();
        }

        public void AddChild(string subKey, Node child)
        {
            _children.Add(subKey, child);
            BindableChildren.Add(child);
        }

        public Node GetChild(string subKey)
        {
            Node child;
            _children.TryGetValue(subKey, out child);
            return child;
        }

        public void RemoveChild(Node child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            if (child.Parent != this)
                throw new InvalidOperationException("Parent mismatch");

            if (!_children.ContainsKey(child.Key))
                throw new InvalidOperationException("No such child");

            _children.Remove(child.Key);
            BindableChildren.Remove(child);
        }

        public void ClearChildren()
        {
            _children.Clear();
            BindableChildren.Clear();
        }

        public override string ToString()
        {
            return "{0} ({1})".F(Key, Type);
        }
    }
}
