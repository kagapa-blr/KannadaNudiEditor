using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
#if !Framework3_5
#endif

namespace KannadaNudiEditor
{
    /// <summary>
    /// Specifies the Percentage value to Double value converter
    /// </summary>
    internal class PercentageDoubleConverter : IValueConverter
    {
        /// <summary>
        /// Converts the percentage value to double.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 100;
        }
        /// <summary>
        /// Converts the double value to percentage.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value * 100;
        }
    }

    /// <summary>
    /// Specifies the Percentage value to String value converter
    /// </summary>
    internal class PercentageStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts the percentage value to string.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Math.Round((double)value);
        }
        /// <summary>
        /// Converts the string value to double.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }



    }

    /// <summary>
    /// Specifies the line spacing toggle converter
    /// </summary>
    internal class LineSpacingToggleConverter : IValueConverter
    {
        /// <summary>
        /// Converts the Line spacing value to Toggle.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double lineSpacing = Math.Round((double)value, 2);
            if (lineSpacing.ToString() == parameter.ToString())
                return true;
            return false;
        }
        /// <summary>
        /// Converts the Toggle value to Line spacing.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is string s && double.TryParse(s, out double lineSpacing))
                return lineSpacing >= 1 ? lineSpacing : 1d;
            return 1d;
        }
    }

    /// <summary>
    /// Specifies the Double value to Percentage converter
    /// </summary>
    internal class DoublePercentageConverter : IValueConverter
    {
        /// <summary>
        /// Converts the percentage value to double.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double factor = (double)value;
            return (int)(factor / 100);
        }
        /// <summary>
        /// Converts the double value to percentage.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value * 100;
        }
    }

    /// <summary>
    /// Specifies the Line spacing value to Toggle converter
    /// </summary>
    internal class ListToggleConverter : IValueConverter
    {
        /// <summary>
        /// Converts the List value to Toggle.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Converts the Toggle value to List.
        /// </summary>
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Only return parameter if the toggle is checked (true)
            if (value is bool boolVal && boolVal && parameter != null)
                return parameter.ToString();
            // Return null if unchecked or invalid
            return null;
        }
    }

    /// <summary>
    /// Specifies the Font color to color and vice versa
    /// </summary>
    internal class FontColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts the specified Font color to color.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo language)
        {
            if (value is Color)
                return value;
            return Color.FromArgb(0, 255, 255, 255);
        }
        /// <summary>
        /// Converts the color to Font color.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns></returns>
        public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo language)
        {
            if (value is Color color)
            {
                // If not fully transparent white, return the color
                if (!(color.A == 0 && color.R == 255 && color.G == 255 && color.B == 255))
                    return color;
            }
            return null;
        }
    }

    internal class LayoutTypeToggleConverter : IValueConverter
    {
        /// <summary>
        /// Converts the percentage value to double.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && parameter != null && value.ToString() == parameter.ToString())
                return true;
            return false;
        }
        /// <summary>
        /// Converts the double value to percentage.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return parameter;
        }
    }
}
