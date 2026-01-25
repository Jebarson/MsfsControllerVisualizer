namespace Msfs.ControllerVisualizer.Converters;

using System.Globalization;
using System.Windows.Data;
using Msfs.ControllerVisualizer.Models;

/// <summary>
/// Converts joystick button identifiers to truncated friendly names.
/// Truncates long text and appends "..." if it exceeds MaxLength.
/// </summary>
public class JoystickButtonTruncateConverter : IValueConverter
{
    private static List<ButtonMapping>? currentMappings;
    private readonly JoystickButtonMappingConverter mappingConverter = new();

    /// <summary>
    /// Gets or sets the maximum length of the output text before truncation.
    /// Default is 15 characters.
    /// </summary>
    public int MaxLength { get; set; } = 15;

    /// <summary>
    /// Sets the current button mappings to use for conversion.
    /// </summary>
    /// <param name="mappings">The list of button mappings.</param>
    public static void SetCurrentMappings(List<ButtonMapping> mappings)
    {
        currentMappings = mappings;
        JoystickButtonMappingConverter.SetCurrentMappings(mappings);
    }

    /// <summary>
    /// Converts a button identifier to its truncated friendly name.
    /// </summary>
    /// <param name="value">The binding source value (not used).</param>
    /// <param name="targetType">The target property type.</param>
    /// <param name="parameter">The button identifier to lookup (e.g., "Joystick_Button_35").</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The truncated friendly name, or empty string if no mapping found.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // First get the full mapped text
        string fullText = this.mappingConverter.Convert(value, targetType, parameter, culture)?.ToString() ?? string.Empty;

        if (string.IsNullOrEmpty(fullText))
            return string.Empty;

        if (fullText.Length <= this.MaxLength)
            return fullText;

        return fullText.Substring(0, this.MaxLength) + "...";
    }

    /// <summary>
    /// Not implemented - conversion back is not supported.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
