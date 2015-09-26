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

        T AddChild(string key, NodeType type, out T outParent)
        {
            var keyList = key.Split(new[] { _separator }, StringSplitOptions.None).ToList();

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

        void ClearChildren(T node)
        {
            node.Children.Cast<T>().Do(child => ClearChildren(child));
            node.ClearChildren();
        }
    }
}
