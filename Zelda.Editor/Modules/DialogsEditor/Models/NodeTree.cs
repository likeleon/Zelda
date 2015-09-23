using System;
using System.Linq;

namespace Zelda.Editor.Modules.DialogsEditor.Models
{
    enum NodeType
    {
        Container,
        RealKey,
        RefKey
    }

    class NodeTree<T> where T : Node, new()
    {
        class Root : Node
        {
        }

        readonly string _separator;
        readonly Node _root = new Root();

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
                node = parent.GetChild(keyList.First());
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
                node = new T();
                node.Parent = parent;
                if (parent.Key.Length > 0)
                    node.Key = parent.Key + _separator + subKey;
                else
                    node.Key = subKey;

                parent = node;
                keyList.RemoveAt(0);
            }

            node.Type = type;
            return node;
        }
    }
}
