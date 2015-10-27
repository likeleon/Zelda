using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Zelda.Editor.Core.Controls
{
    class EditableTextBlockAdorner : Adorner
    {
        public event EventHandler<string> TextBoxEditEnding;

        static readonly double ExtraHeight = 10;
        static readonly double ExtraWidth = 10;

        VisualCollection _collection;
        EditableTextBlock _textBlock;

        public TextBox TextBox { get; private set; }
        protected override int VisualChildrenCount { get { return _collection.Count; } }

        public EditableTextBlockAdorner(EditableTextBlock adornedElement)
            : base(adornedElement)
        {
            _collection = new VisualCollection(this);
            _textBlock = adornedElement;

            BuildTextBox(adornedElement);
        }

        void BuildTextBox(EditableTextBlock adornedElement)
        {
            TextBox = new TextBox();
            var binding = new Binding("Text") { Source = adornedElement };
            TextBox.SetBinding(TextBox.TextProperty, binding);
            TextBox.VerticalAlignment = VerticalAlignment.Stretch;
            TextBox.VerticalContentAlignment = VerticalAlignment.Center;
            TextBox.MaxLength = adornedElement.MaxLength;
            TextBox.KeyUp += OnTextBoxKeyUp;

            _collection.Add(TextBox);
        }

        void OnTextBoxKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox.Text = TextBox.Text.Replace("\r\n", string.Empty);

                var expression = TextBox.GetBindingExpression(TextBox.TextProperty);
                if (expression != null)
                    expression.UpdateSource();

                if (TextBoxEditEnding != null)
                    TextBoxEditEnding(this, TextBox.Text);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            TextBox.Arrange(new Rect(0, -ExtraHeight / 2.0, DesiredSize.Width + ExtraWidth, DesiredSize.Height + ExtraHeight));
            TextBox.Focus();
            return finalSize;
        }

        protected override Visual GetVisualChild(int index) { return _collection[index]; }

        public event RoutedEventHandler TextBoxLostFocus
        {
            add { TextBox.LostFocus += value; }
            remove { TextBox.LostFocus -= value; }
        }

        public event KeyEventHandler TextBoxKeyUp
        {
            add { TextBox.KeyUp += value; }
            remove { TextBox.KeyUp -= value; }
        }
    }
}