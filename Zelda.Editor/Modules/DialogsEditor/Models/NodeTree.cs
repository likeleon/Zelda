using System;
using System.Linq;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.Models
{
    enum NodeType
    {
        Container,
        RealKey,
        RefKey
    }

    class NodeTree
    {
        readonly string _separator;
        readonly Node _root = new Node();

        public Node Root { get { return _root; } }

        public NodeTree(string separator)
        {
            _separator = separator;
        }

        public Node AddKey(string key)
        {
            Node parent;
            return AddKey(key, out parent);
        }

        public Node AddKey(string key, out Node parent)
        {
            return AddChild(key, NodeType.RealKey, out parent);
        }

        Node AddChild(string key, NodeType type, out Node outParent)
        {
            var keyList = key.Split(new[] { _separator }, StringSplitOptions.None).ToList();

            var parent = _root;
            var node = parent;
            while (node != null && keyList.Count > 0)
            {
                node = parent.GetChild(keyList.First()) as Node;
                if (node != null)
                {
                    parent = node;
                    keyList.RemoveAt(0);
                }
            }

            // 이미 노드가 존재
            if (node != null)
            {
                outParent = null;
                return null;
            }

            outParent = parent;

            // 누락된 노드들 추가
            while (keyList.Count > 0)
            {
                var subKey = keyList.First();
                node = new Node();
                node.Parent = parent;
                if (!parent.Key.IsNullOrEmpty())
                    node.Key = parent.Key + _separator + subKey;
                else
                    node.Key = subKey;
                parent.AddChild(subKey, node);

                parent = node;
                keyList.RemoveAt(0);
            }

            node.Type = type;
            return node;
        }

        public void Clear()
        {
            ClearChildren(_root);
        }

        void ClearChildren(Node node)
        {
            node.Children.Cast<Node>().Do(child => ClearChildren(child));
            node.ClearChildren();
        }
    }
}
