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

    class NodeTree<T> where T : Node, new()
    {
        readonly string _separator;
        readonly T _root = new T();

        public T Root { get { return _root; } }

        public NodeTree(string separator)
        {
            _separator = separator;
        }

        public T AddKey(string key)
        {
            T parent;
            return AddKey(key, out parent);
        }

        public T AddKey(string key, out T parent)
        {
            return AddChild(key, NodeType.RealKey, out parent);
        }

        public T AddRef(string key)
        {
            T parent;
            return AddRef(key, out parent);
        }

        public T AddRef(string key, out T parent)
        {
            return AddChild(key, NodeType.RefKey, out parent);
        }

        T AddChild(string key, NodeType type, out T outParent)
        {
            var keyList = key.Split(new[] { _separator }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var parent = _root;
            var node = parent;
            while (node != null && keyList.Count > 0)
            {
                node = parent.GetChild(keyList.First()) as T;
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
                return node;
            }

            outParent = parent;

            // 누락된 노드들 추가
            while (keyList.Count > 0)
            {
                var subKey = keyList.First();
                node = new T();
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

        public bool CanRemoveRef(T node, out T childToRemove)
        {
            return CanRemoveChild(node, NodeType.RefKey, out childToRemove);
        }

        public bool RemoveRef(T node, bool keepKey)
        {
            return RemoveChild(node, NodeType.RefKey, keepKey);
        }

        bool RemoveChild(T node, NodeType type, bool keepKey = false)
        {
            T childToRemove;
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

        public bool CanRemoveKey(T node, out T childToRemove)
        {
            return CanRemoveChild(node, NodeType.RefKey, out childToRemove);
        }

        bool CanRemoveChild(T node, NodeType type, out T childToRemove)
        {
            if (node.Children.Any() || node.Type != type)
            {
                childToRemove = null;
                return false;
            }

            // 실제 삭제해야할 노드를 찾습니다
            var parent = (T)node.Parent;
            childToRemove = node;
            while (parent != Root && parent != null &&
                   parent.Type == NodeType.Container && parent.Children.Count() == 1)
            {
                childToRemove = parent;
                parent = (T)childToRemove.Parent;
            }
            return true;
        }

        public bool RemoveKey(T node)
        {
            return RemoveChild(node, NodeType.RealKey);
        }

        public void Clear()
        {
            ClearChildren(_root);
        }

        void ClearChildren(T node)
        {
            node.Children.Values.Do(child => ClearChildren((T)child));
            node.ClearChildren();
        }

        public T Find(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var node = Root;
            var subKeys = key.Split(new[] { _separator }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var subKey in subKeys)
            {
                node = (T)node.GetChild(subKey);
                if (node == null)
                    return null;
            }
            return node;
        }
    }
}
