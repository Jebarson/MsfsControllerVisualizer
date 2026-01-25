// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Converters;

using System.Globalization;
using System.Windows.Data;
using Msfs.ControllerVisualizer.Models;

/// <summary>
/// Converts joystick button identifiers to their mapped friendly names from MSFS configuration.
/// Supports multiple mappings per button by joining them with commas.
/// </summary>
public class JoystickButtonMappingConverter : IValueConverter
{
    private static List<ButtonMapping>? currentMappings;

    /// <summary>
    /// Sets the current button mappings to use for conversion.
    /// </summary>
    /// <param name="mappings">The list of button mappings.</param>
    public static void SetCurrentMappings(List<ButtonMapping> mappings)
    {
        currentMappings = mappings;
    }

    /// <summary>
    /// Converts a button identifier to its friendly name.
    /// </summary>
    /// <param name="value">The binding source value (not used).</param>
    /// <param name="targetType">The target property type.</param>
    /// <param name="parameter">The button identifier to lookup (e.g., "Joystick_Button_35").</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>The friendly name of the mapped action, or empty string if no mapping found.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter == null)
            return string.Empty;

        string bindingPath = parameter.ToString() ?? string.Empty;

        // Look for mapping by ButtonId (which matches the XAML ConverterParameter)
        if (currentMappings != null)
        {
            List<ButtonMapping> maps = currentMappings
                .Where(m => m.ButtonId.Equals(bindingPath, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (maps.Count == 1)
                return maps[0].FriendlyName;

            if (maps.Count > 1)
            {
                // Multiple mappings - join with comma
                return string.Join(", ", maps.Select(m => m.FriendlyName));
            }
        }

        // No mapping found - return empty string
        return string.Empty;
    }

    /// <summary>
    /// Not implemented - conversion back is not supported.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
