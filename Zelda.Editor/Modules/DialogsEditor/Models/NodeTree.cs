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

        public Node AddRef(string key)
        {
            Node parent;
            return AddRef(key, out parent);
        }

        public Node AddRef(string key, out Node parent)
        {
            return AddChild(key, NodeType.RefKey, out parent);
        }

        Node AddChild(string key, NodeType type, out Node outParent)
        {
            var keyList = key.Split(new[] { _separator }, StringSplitOptions.RemoveEmptyEntries).ToList();

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
                if (type == NodeType.RefKey || node.Type == NodeType.Container)
                    node.Type = type;

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

        public bool RemoveRef(Node node, bool keepKey)
        {
            return RemoveChild(node, NodeType.RefKey, keepKey);
        }

        bool RemoveChild(Node node, NodeType type, bool keepKey)
        {
            Node childToRemove;
            if (!CanRemoveChild(node, type, out childToRemove))
            {
                if (type == node.Type)
                {
                    if (type == NodeType.RefKey && keepKey)
                        node.Type = NodeType.RealKey;
                    else
                        node.Type = NodeType.Container;
                }
                return true;
            }

            if (type == NodeType.RefKey && keepKey)
            {
                node.Type = NodeType.RealKey;
                return true;
            }

            childToRemove.Parent.RemoveChild(childToRemove);
            return true;
        }

        public bool CanRemoveKey(Node node, out Node childToRemove)
        {
            return CanRemoveChild(node, NodeType.RefKey, out childToRemove);
        }

        bool CanRemoveChild(Node node, NodeType type, out Node childToRemove)
        {
            if (node.ChildrenCount > 0 || node.Type != type)
            {
                childToRemove = null;
                return false;
            }

            // 실제 삭제해야할 노드를 찾습니다
            var parent = node.Parent;
            childToRemove = node;
            while (parent != Root && parent != null &&
                   parent.Type == NodeType.Container && parent.ChildrenCount == 1)
            {
                childToRemove = parent;
                parent = childToRemove.Parent;
            }
            return true;
        }

        public void Clear()
        {
            ClearChildren(_root);
        }

        void ClearChildren(Node node)
        {
            node.Children.Do(child => ClearChildren(child));
            node.ClearChildren();
        }

        public Node Find(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var node = Root;
            var subKeys = key.Split(new[] { _separator }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var subKey in subKeys)
            {
                node = node.GetChild(subKey);
                if (node == null)
                    return null;
            }
            return node;
        }
    }
}
