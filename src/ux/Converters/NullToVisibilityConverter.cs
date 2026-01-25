using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Msfs.ControllerVisualizer.Converters;

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isInverted = parameter?.ToString() == "Inverted";
        bool isNull = value == null;

        if (isInverted)
            return isNull ? Visibility.Visible : Visibility.Collapsed;
        
        return isNull ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
