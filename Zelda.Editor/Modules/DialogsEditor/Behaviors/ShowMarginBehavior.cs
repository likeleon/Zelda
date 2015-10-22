using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Zelda.Editor.Modules.DialogsEditor.Behaviors
{
    class ShowMarginBehavior : Behavior<TextBox>
    {
        static readonly Pen DefaultPenOnEnabled = new Pen(Brushes.Blue, 1.0);
        static readonly Pen DefaultPenOnDisabled = new Pen(Brushes.Gray, 1.0);
        MarginAdorner _adorner;

        public bool DisplayMargin
        {
            get { return (bool)GetValue(DisplayMarginProperty); }
            set { SetValue(DisplayMarginProperty, value); }
        }

        public static readonly DependencyProperty DisplayMarginProperty =
            DependencyProperty.Register("DisplayMargin", typeof(bool), typeof(ShowMarginBehavior), 
                new PropertyMetadata(false, OnOptionChanged));

        public int Margin
        {
            get { return (int)GetValue(MarginProperty); }
            set { SetValue(MarginProperty, value); }
        }

        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.Register("Margin", typeof(int), typeof(ShowMarginBehavior), 
                new PropertyMetadata(0, OnOptionChanged));

        public Pen PenOnEnabled
        {
            get { return (Pen)GetValue(PenOnEnabledProperty); }
            set { SetValue(PenOnEnabledProperty, value); }
        }

        public static readonly DependencyProperty PenOnEnabledProperty =
            DependencyProperty.Register("PenOnEnabled", typeof(Pen), typeof(ShowMarginBehavior), 
                new PropertyMetadata(DefaultPenOnEnabled, OnOptionChanged));

        public Pen PenOnDisabled
        {
            get { return (Pen)GetValue(PenOnDisabledProperty); }
            set { SetValue(PenOnDisabledProperty, value); }
        }

        public static readonly DependencyProperty PenOnDisabledProperty =
            DependencyProperty.Register("PenOnDisabled", typeof(Pen), typeof(ShowMarginBehavior), 
                new PropertyMetadata(DefaultPenOnDisabled, OnOptionChanged));

        static void OnOptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as ShowMarginBehavior;
            if (behavior._adorner != null && behavior.AssociatedObject != null)
                behavior._adorner.InvalidateVisual();
        }

        protected override void OnAttached()
        {
            _adorner = new MarginAdorner(this);

            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.IsEnabledChanged += (_, e) => _adorner.InvalidateVisual();
        }

        void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(AssociatedObject);
            layer.Add(_adorner);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        class MarginAdorner : Adorner
        {
            readonly ShowMarginBehavior _behavior; 
            TextBox AdornedTextBox { get { return AdornedElement as TextBox; } }

            public MarginAdorner(ShowMarginBehavior behavior)
                : base(behavior.AssociatedObject)
            {
                _behavior = behavior;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);
                
                if (!_behavior.DisplayMargin)
                    return;

                var x = CalculateMarginX();
                var pen = AdornedTextBox.IsEnabled ? _behavior.PenOnEnabled : _behavior.PenOnDisabled;

                var halfPenWidth = pen.Thickness / 2.0;
                var guidlineSet = new GuidelineSet();
                guidlineSet.GuidelinesX.Add(x + halfPenWidth);

                drawingContext.PushGuidelineSet(guidlineSet);
                drawingContext.DrawLine(pen, new Point(x, 0.0), new Point(x, AdornedTextBox.ActualHeight));
                drawingContext.Pop();
            }

            double CalculateMarginX()
            {
                var text = new FormattedText(
                    new string('a', _behavior.Margin),
                    CultureInfo.CurrentUICulture,
                    AdornedTextBox.FlowDirection,
                    new Typeface(AdornedTextBox.FontFamily, AdornedTextBox.FontStyle, AdornedTextBox.FontWeight, AdornedTextBox.FontStretch),
                    AdornedTextBox.FontSize,
                    AdornedTextBox.Foreground);
                return AdornedTextBox.Padding.Left + text.Width;
            }
        }
    }
}
