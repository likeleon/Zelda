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
        string _key;
        Node _parent;

        public string Key
        {
            get { return _key; }
            set { this.SetProperty(ref _key, value); }
        }

        public NodeType Type { get; set; }

        public Node Parent
        {
            get { return _parent; }
            set { this.SetProperty(ref _parent, value); }
        }

        public IEnumerable<Node> Children { get { return _children.Values; } }

        public void AddChild(string subKey, Node child)
        {
            _children.Add(subKey, child);
            NotifyOfPropertyChange(() => Children);
        }

        public Node GetChild(string subKey)
        {
            Node child;
            _children.TryGetValue(subKey, out child);
            return child;
        }

        public void ClearChildren()
        {
            _children.Clear();
            NotifyOfPropertyChange(() => Children);
        }

        public override string ToString()
        {
            return "{0} ({1})".F(Key, Type);
        }
    }
}
