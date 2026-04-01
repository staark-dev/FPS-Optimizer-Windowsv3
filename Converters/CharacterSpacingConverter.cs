using System;
using System.Globalization;
using System.Windows.Data;

namespace FPSOptimizer.Converters
{
    public class CharacterSpacingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";

            int spacing = 1;

            if (parameter != null && int.TryParse(parameter.ToString(), out int raw))
            {
                spacing = Math.Max(1, raw / 100); // 150 → 1, 300 → 3
            }

            return string.Join(
                new string(' ', spacing),
                value?.ToString()?.ToCharArray() ?? Array.Empty<char>()
            );
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value;
    }
}