using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using WpfUserControl = System.Windows.Controls.UserControl;
using Msfs.ControllerVisualizer.Models;

namespace Msfs.ControllerVisualizer.Converters;

public class ControllerVisualConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] == null || values[1] == null)
            return null;

        ControllerDefinition? controllerDef = values[0] as ControllerDefinition;
        System.Collections.Generic.List<ButtonMapping>? buttonMappings = values[1] as System.Collections.Generic.List<ButtonMapping>;

        if (controllerDef == null || buttonMappings == null)
            return null;

        try
        {
            string visualPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Controllers",
                controllerDef.VisualFile
            );

            if (!File.Exists(visualPath))
            {
                System.Diagnostics.Debug.WriteLine($"Visual file not found: {visualPath}");
                return null;
            }

            // Set the current mappings for the converters to use BEFORE loading XAML
            JoystickButtonMappingConverter.SetCurrentMappings(buttonMappings);
            JoystickButtonTruncateConverter.SetCurrentMappings(buttonMappings);

            // Create a parser context with mappings for namespace prefixes
            ParserContext context = new();
            context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            context.XmlnsDictionary.Add("converters", "clr-namespace:Msfs.ControllerVisualizer.Converters;assembly=Msfs.ControllerVisualizer");

            using FileStream stream = File.OpenRead(visualPath);
            WpfUserControl visual = (WpfUserControl)XamlReader.Load(stream, context);

            // Set a simple DataContext - the converter will handle the actual mapping lookup
            visual.DataContext = new { };

            return visual;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading controller visual: {ex.Message}");
            return null;
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
