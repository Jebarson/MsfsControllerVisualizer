// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Converters;

using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Markup;
using Msfs.ControllerVisualizer.Models;
using WpfUserControl = System.Windows.Controls.UserControl;

/// <summary>
/// Converts a controller definition and button mappings to a visual XAML user control.
/// Dynamically loads and renders XAML files based on the controller definition.
/// </summary>
public class ControllerVisualConverter : IMultiValueConverter
{
    private static readonly string ConvertersXmlNamespace =
        $"clr-namespace:Msfs.ControllerVisualizer.Converters;assembly={typeof(JoystickButtonMappingConverter).Assembly.GetName().Name}";

    /// <summary>
    /// Converts controller definition and button mappings to a rendered visual user control.
    /// </summary>
    /// <param name="values">Array containing [ControllerDefinition, List&lt;ButtonMapping&gt;].</param>
    /// <param name="targetType">The target type for conversion.</param>
    /// <param name="parameter">Converter parameter (not used).</param>
    /// <param name="culture">Culture information for conversion.</param>
    /// <returns>A UserControl representing the controller visual, or null if conversion fails.</returns>
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] == null || values[1] == null)
        {
            return null;
        }

        ControllerDefinition? controllerDef = values[0] as ControllerDefinition;
        System.Collections.Generic.List<ButtonMapping>? buttonMappings = values[1] as System.Collections.Generic.List<ButtonMapping>;

        if (controllerDef == null || buttonMappings == null)
        {
            return null;
        }

        try
        {
            string visualPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "Controllers",
                controllerDef.VisualFile);

            if (!File.Exists(visualPath))
            {
                System.Diagnostics.Debug.WriteLine($"Visual file not found: {visualPath}");
                return null;
            }

            // Set the current mappings for the converters to use BEFORE loading XAML
            JoystickButtonMappingConverter.SetCurrentMappings(buttonMappings);
            JoystickButtonTruncateConverter.SetCurrentMappings(buttonMappings);

            // Create a parser context with mappings for namespace prefixes
            using FileStream stream = File.OpenRead(visualPath);
            ParserContext context = new()
            {
                BaseUri = new Uri(visualPath, UriKind.Absolute),
            };
            context.XmlnsDictionary.Add(string.Empty, "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            context.XmlnsDictionary.Add("converters", ConvertersXmlNamespace);

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

    /// <summary>
    /// Not implemented - conversion back is not supported.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetTypes">The requested target types.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">Culture information for conversion.</param>
    /// <returns>This method does not return a value because it always throws.</returns>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
