﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Zelda.Game;

namespace Zelda.Editor.Core.Controls
{
    class EditableTextBlock : TextBlock
    {
        EditableTextBlockAdorner _adorner;

        public bool IsInEditMode
        {
            get { return (bool)GetValue(IsInEditModeProperty); }
            set { SetValue(IsInEditModeProperty, value); }
        }

        public static readonly DependencyProperty IsInEditModeProperty =
            DependencyProperty.Register("IsInEditMode", typeof(bool), typeof(EditableTextBlock), new UIPropertyMetadata(false, IsInEditModeUpdate));

        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLength", typeof(int), typeof(EditableTextBlock), new UIPropertyMetadata(0));

        static void IsInEditModeUpdate(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = obj as EditableTextBlock;
            if (textBlock == null)
                return;

            var layer = AdornerLayer.GetAdornerLayer(textBlock);
            if (textBlock.IsInEditMode)
            {
                if (textBlock._adorner == null)
                {
                    textBlock._adorner = new EditableTextBlockAdorner(textBlock);
                    textBlock._adorner.TextBoxKeyUp += textBlock.TextBoxKeyUp;
                    textBlock._adorner.TextBoxLostFocus += textBlock.TextBoxLostFocus;
                }
                textBlock._adorner.TextBox.SelectAll();
                layer.Add(textBlock._adorner);
            }
            else
            {
                var adorners = layer.GetAdorners(textBlock);
                if (adorners != null)
                    adorners.OfType<EditableTextBlockAdorner>().Do(a => layer.Remove(a));

                var expression = textBlock.GetBindingExpression(TextProperty);
                if (expression != null)
                    expression.UpdateTarget();
            }
        }

        void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            IsInEditMode = false;
        }

        void TextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                IsInEditMode = false;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
                IsInEditMode = true;
            else if (e.ClickCount == 2)
                IsInEditMode = true;
        }
    }
}
