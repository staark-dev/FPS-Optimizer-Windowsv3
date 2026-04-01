using System.Windows;
using System.Windows.Controls;

namespace FPSOptimizer.Helpers
{
    public static class TextBlockExtensions
    {
        public static readonly DependencyProperty TextTransformProperty =
            DependencyProperty.RegisterAttached(
                "TextTransform",
                typeof(string),
                typeof(TextBlockExtensions),
                new PropertyMetadata(null, OnTextTransformChanged));

        public static void SetTextTransform(DependencyObject element, string value)
        {
            element.SetValue(TextTransformProperty, value);
        }

        public static string GetTextTransform(DependencyObject element)
        {
            return (string)element.GetValue(TextTransformProperty);
        }

        private static void OnTextTransformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock tb)
            {
                tb.Loaded += (s, _) =>
                {
                    if (tb.Text == null) return;

                    switch (e.NewValue?.ToString())
                    {
                        case "Uppercase":
                            tb.Text = tb.Text.ToUpper();
                            break;
                        case "Lowercase":
                            tb.Text = tb.Text.ToLower();
                            break;
                    }
                };
            }
        }
    }
}