using System;
using System.Collections.Generic;
using Zelda.Editor.Core.Mods;
using Zelda.Game;
using ModResources = Zelda.Editor.Core.Mods.ModResources;

namespace Zelda.Editor.Modules.ResourceSelector
{
    class ResourceModel
    {
        readonly Dictionary<string, Item> _items = new Dictionary<string, Item>();

        public IMod Mod { get; private set; }
        public ResourceType ResourceType { get; private set; }
        public ModResources Resources { get { return Mod.Resources; } }
        public InvisibleRootItem InvisibleRootItem { get; private set; }

        public ResourceModel(IMod mod, ResourceType resourceType)
        {
            if (mod == null)
                throw new ArgumentNullException("mod");

            Mod = mod;
            ResourceType = resourceType;
            InvisibleRootItem = new InvisibleRootItem();

            Resources.GetElements(ResourceType).Do(id => AddElement(id));

            Resources.ElementAdded += Resources_ElementAdded;
            Resources.ElementRemoved += Resources_ElementRemoved;
            Resources.ElementRenamed += Resources_ElementRenamed;
            Resources.ElementDescriptionChanged += Resources_ElementDescriptionChanged;
        }

        void Resources_ElementAdded(object sender, ElementAddedEventArgs e)
        {
            if (e.ResourceType == ResourceType)
                AddElement(e.Id);
        }

        void Resources_ElementRemoved(object sender, ElementRemovedEventArgs e)
        {
            if (e.ResourceType == ResourceType)
                RemoveElement(e.Id);
        }

        void Resources_ElementRenamed(object sender, ElementRenamedEventArgs e)
        {
            if (e.ResourceType != ResourceType)
                return;

            var item = GetElement<ElementItem>(e.OldId);
            if (item == null)
                return;

            item.Id = e.NewId;

            _items.Remove(e.OldId);
            _items.Add(e.NewId, item);
        }

        void Resources_ElementDescriptionChanged(object sender, ElementDescriptionChangedEventArgs e)
        {
            if (e.ResourceType != ResourceType)
                return;

            var item = GetElement<ElementItem>(e.Id);
            if (item == null)
                return;

            item.Description = e.Description;
        }

        void AddElement(string elementId)
        {
            var files = elementId.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            Item parent = InvisibleRootItem;
            foreach (var file in files.WithoutLast())
                parent = FindOrCreateDirectoryItem(parent, file);

            var item = CreateElementItem(elementId);
            parent.AppendChild(item);
        }

        DirectoryItem FindOrCreateDirectoryItem(Item parent, string dirName)
        {
            for (int i = 0; i < InvisibleRootItem.ChildrenCount; ++i)
            {
                var childDir = parent.GetChild<DirectoryItem>(i);
                if (childDir == null)
                    continue;

                var compareResult = string.Compare(childDir.Name, dirName);
                if (compareResult == 0)
                    return childDir;

                if (compareResult > 0)
                {
                    childDir = CreateDirectoryItem(dirName);
                    parent.InsertChild(i, childDir);
                    return childDir;
                }
            }

            var child = CreateDirectoryItem(dirName);
            parent.AppendChild(child);
            return child;
        }

        DirectoryItem CreateDirectoryItem(string dirName)
        {
            return new DirectoryItem(dirName);
        }

        ElementItem CreateElementItem(string elementId)
        {
            var description = Resources.GetDescription(ResourceType, elementId);
            var item = new ElementItem() { Id = elementId, Description = description };
            _items.Add(elementId, item);
            return item;
        }

        void RemoveElement(string elementId)
        {
            var item = _items[elementId];
            item.Parent.RemoveChild(item);

            _items.Remove(elementId);
        }

        public void AddSpecialValue(string id, string text, int index)
        {
            var item = new SpecialValueItem() { Id = id, Text = text };
            _items.Add(id, item);
            InvisibleRootItem.InsertChild(index, item);
        }

        public void RemoveId(string id)
        {
            var item = GetElement<ElementItem>(id);
            if (item != null)
                RemoveElement(id);
        }

        public T GetElement<T>(string id) where T : Item
        {
            Item item;
            if (!_items.TryGetValue(id, out item))
                return null;

            return item as T;
        }
    }
}
