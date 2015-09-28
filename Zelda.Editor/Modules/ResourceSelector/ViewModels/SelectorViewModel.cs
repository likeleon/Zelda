using Caliburn.Micro;
using System.Collections.Generic;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Modules.ResourceSelector.Models;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceSelector.ViewModels
{
    class SelectorViewModel : PropertyChangedBase
    {
        Item _selectedItem;

        public ResourceModel Model { get; private set; }
        public Item SelectedItem
        {
            get { return _selectedItem; }
            set { this.SetProperty(ref _selectedItem, value); }
        }

        public IEnumerable<Item> RootItems { get { return Model.InvisibleRootItem.Children; } }

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
