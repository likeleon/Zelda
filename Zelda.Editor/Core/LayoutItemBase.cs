using Caliburn.Micro;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Zelda.Editor.Core
{
    public abstract class LayoutItemBase : Screen, ILayoutItem
    {
        private readonly Guid _id = Guid.NewGuid();

        public abstract ICommand CloseCommand { get; }

        [Browsable(false)]
        public Guid Id
        {
            get { return _id; }
        }

        [Browsable(false)]
        public string ContentId
        {
            get { return _id.ToString(); }
        }
        
        [Browsable(false)]
        public virtual Uri IconSource
        {
            get { return null; }
        }

        private bool _isSelected;

        [Browsable(false)]
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.SetProperty(ref _isSelected, value); }
        }
    }
}
