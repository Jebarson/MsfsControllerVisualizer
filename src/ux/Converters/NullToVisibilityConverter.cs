// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Converters;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

/// <summary>
/// Converts null values to Visibility.Collapsed and non-null values to Visibility.Visible.
/// Supports an "Inverted" parameter to reverse the logic.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a null or non-null value to a Visibility enumeration value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The target type for conversion.</param>
    /// <param name="parameter">Optional "Inverted" parameter to reverse the logic.</param>
    /// <param name="culture">Culture information for conversion.</param>
    /// <returns>Visibility.Collapsed for null values (or Visibility.Visible if Inverted), otherwise Visibility.Visible (or Visibility.Collapsed if Inverted).</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isInverted = parameter?.ToString() == "Inverted";
        bool isNull = value == null;

        if (isInverted)
        {
            return isNull ? Visibility.Visible : Visibility.Collapsed;
        }

        return isNull ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>
    /// Not implemented - conversion back is not supported.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The requested target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">Culture information for conversion.</param>
    /// <returns>This method does not return a value because it always throws.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
