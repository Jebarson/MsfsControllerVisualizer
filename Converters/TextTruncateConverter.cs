namespace Msfs.ControllerVisualizer.Converters;

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

public class TextTruncateConverter : IValueConverter
{
    public int MaxLength { get; set; } = 20;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;

        string text = value.ToString() ?? string.Empty;

        if (string.IsNullOrEmpty(text))
            return string.Empty;

        if (text.Length <= this.MaxLength)
            return text;

        return text.Substring(0, this.MaxLength) + "...";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class TextToTooltipConverter : IValueConverter
{
    public int MinLengthForTooltip { get; set; } = 20;

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        string text = value.ToString() ?? string.Empty;

        if (string.IsNullOrEmpty(text) || text.Length <= this.MinLengthForTooltip)
            return null;

        return text;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
