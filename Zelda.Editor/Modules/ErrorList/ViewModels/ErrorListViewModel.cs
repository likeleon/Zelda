using Caliburn.Micro;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Zelda.Editor.Core;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.ErrorList.ViewModels
{
    [Export(typeof(IErrorList))]
    public class ErrorListViewModel : Tool, IErrorList
    {
        private readonly BindableCollection<ErrorListItem> _items = new BindableCollection<ErrorListItem>();

        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Bottom; }
        }

        public IObservableCollection<ErrorListItem> Items
        {
            get { return _items; }
        }

        public IEnumerable<ErrorListItem> FilteredItems
        {
            get
            {
                var items = _items.AsEnumerable();
                if (!ShowErrors)
                    items = items.Where(x => x.ItemType != ErrorListItemType.Error);
                if (!ShowWarnings)
                    items = items.Where(x => x.ItemType != ErrorListItemType.Warning);
                if (!ShowMessages)
                    items = items.Where(x => x.ItemType != ErrorListItemType.Message);
                return items;
            }
        }

        private bool _showErrors = true;
        public bool ShowErrors
        {
            get { return _showErrors; }
            set
            {
                if (this.SetProperty(ref _showErrors, value))
                    NotifyOfPropertyChange("FilteredItems");
            }
        }

        private bool _showWarnings = true;
        public bool ShowWarnings
        {
            get { return _showWarnings; }
            set
            {
                if (this.SetProperty(ref _showWarnings, value))
                    NotifyOfPropertyChange("FilteredItems");
            }
        }

        private bool _showMessages = true;
        public bool ShowMessages
        {
            get { return _showMessages; }
            set
            {
                if (this.SetProperty(ref _showMessages, value))
                    NotifyOfPropertyChange("FilteredItems");
            }
        }

        public ErrorListViewModel()
        {
            DisplayName = "Error List";

            _items.CollectionChanged += (sender, e) =>
            {
                NotifyOfPropertyChange(() => FilteredItems);
            };
        }

        public void AddItem(ErrorListItemType itemType, string description,
            string path = null, int? line = null, int? column = null,
            System.Action onClick = null)
        {
            Items.Add(new ErrorListItem(itemType, Items.Count + 1, description, path, line, column)
            {
                OnClick = onClick
            });
        }
    }
}
