using Caliburn.Micro;
using System;
using System.Collections.Generic;
using Zelda.Editor.Core;
using Zelda.Editor.Modules.Mods.Models;
using Zelda.Editor.Modules.ResourceSelector.Models;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceSelector.ViewModels
{
    class SelectorViewModel : PropertyChangedBase
    {
        public event EventHandler<Item> SelectedItemChanged;

        public ResourceModel Model { get; }
        public Item SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (this.SetProperty(ref _selectedItem, value))
                    SelectedItemChanged?.Invoke(this, value);
            }
        }

        public IEnumerable<Item> RootItems => Model.InvisibleRootItem.Children;

        Item _selectedItem;

        public SelectorViewModel(IMod mod, ResourceType resourceType)
        {
            Model = new ResourceModel(mod, resourceType);
        }

        public void AddSpecialValue(string id, string text, int index)
        {
            Model.AddSpecialValue(id, text, index);
        }

        public void RemoveId(string id)
        {
            Model.RemoveId(id);
        }

        public void SetSelectedId(string elementId)
        {
            var item = Model.GetElement<Item>(elementId);
            if (item != null)
                SelectedItem = item;
        }
    }
}
