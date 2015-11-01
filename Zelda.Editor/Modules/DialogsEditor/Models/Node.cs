using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Primitives;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.Models
{
    abstract class Node : PropertyChangedBase
    {
        string _key = "";
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

        public ObservableDictionary<string, Node> Children { get; private set; }

        public Node()
        {
            Children = new ObservableDictionary<string, Node>();
        }

        public void AddChild(string subKey, Node child)
        {
            Children.Add(subKey, child);
        }

        public Node GetChild(string subKey)
        {
            Node child;
            Children.TryGetValue(subKey, out child);
            return child;
        }

        public void RemoveChild(Node child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            if (child.Parent != this)
                throw new InvalidOperationException("Parent mismatch");

            var item = Children.FirstOrDefault(kvp => kvp.Value == child);
            if (item.Equals(default(KeyValuePair<string, Node>)))
                throw new InvalidOperationException("No such child");

            Children.Remove(item.Key);
        }

        public void ClearChildren()
        {
            Children.Clear();
        }

        public override string ToString()
        {
            return "{0} ({1})".F(Key, Type);
        }
    }
}
