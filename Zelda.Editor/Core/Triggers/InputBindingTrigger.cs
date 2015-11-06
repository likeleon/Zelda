using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Zelda.Editor.Core.Triggers
{
    class InputBindingTrigger : TriggerBase<FrameworkElement>, ICommand
    {
        public InputBinding InputBinding
        {
            get { return (InputBinding)GetValue(InputBindingProperty); }
            set { SetValue(InputBindingProperty, value); }
        }

        public static readonly DependencyProperty InputBindingProperty =
            DependencyProperty.Register("InputBinding", typeof(InputBinding), typeof(InputBindingTrigger), new UIPropertyMetadata(null));


        protected override void OnAttached()
        {
            if (InputBinding != null)
            {
                InputBinding.Command = this;
                AssociatedObject.InputBindings.Add(InputBinding);
            }
            base.OnAttached();
        }

        #region ICommand
        public event EventHandler CanExecuteChanged = delegate { };

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            InvokeActions(parameter);
        }
        #endregion
    }
}
