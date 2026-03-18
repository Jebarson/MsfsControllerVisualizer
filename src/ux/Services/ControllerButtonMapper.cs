// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Services;

using System.Xml.Linq;
using Msfs.ControllerVisualizer.Models;

/// <summary>
/// Provides services for mapping controller buttons to MSFS commands from XML configuration.
/// Extracts button bindings and their associated actions from parsed device XML elements.
/// </summary>
public class ControllerButtonMapper
{
    private readonly string keyPrefix = "KEY_";

    /// <summary>
    /// Maps all button bindings from a device XML element to a list of button mappings.
    /// </summary>
    /// <param name="deviceElement">The XML element containing the device configuration.</param>
    /// <param name="controllerDefinition">The controller definition (currently unused but reserved for future enhancements).</param>
    /// <returns>A list of button mappings extracted from the configuration.</returns>
    public List<ButtonMapping> MapButtons(XElement deviceElement, ControllerDefinition controllerDefinition)
    {
        List<ButtonMapping> mappings = new();

        try
        {
            IEnumerable<XElement> contexts = deviceElement.Descendants("Context");

            foreach (XElement context in contexts)
            {
                IEnumerable<XElement> actions = context.Elements("Action");

                foreach (XElement action in actions)
                {
                    string? actionName = action.Attribute("ActionName")?.Value;
                    XElement? primaryKeys = action.Element("Primary");

                    if (primaryKeys != null && !string.IsNullOrEmpty(actionName))
                    {
                        IEnumerable<XElement> keys = primaryKeys.Elements("KEY");

                        foreach (XElement key in keys)
                        {
                            string? keyInfo = key.Attribute("Information")?.Value;

                            if (!string.IsNullOrEmpty(keyInfo))
                            {
                                // Convert "Joystick Button 35" to "Joystick_Button_35"
                                // Convert "Joystick Pov Up" to "Joystick_Pov_Up"
                                // For axes: "Joystick L-Axis X " to "Joystick_L-Axis_X" (trim trailing space, then replace spaces with underscores)
                                string identifier = this.NormalizeIdentifier(keyInfo);

                                ButtonMapping mapping = new()
                                {
                                    ButtonId = identifier,
                                    MsfsCommand = actionName,
                                    FriendlyName = this.ConvertActionToFriendly(actionName)
                                };

                                mappings.Add(mapping);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error mapping buttons: {ex.Message}");
        }

        return mappings;
    }

    /// <summary>
    /// Normalizes a key/axis identifier from the XML format to internal format.
    /// Handles both buttons and axes with their different naming patterns.
    /// </summary>
    /// <param name="keyInfo">The raw identifier from XML (e.g., "Joystick Button 35" or "Joystick L-Axis X ").</param>
    /// <returns>Normalized identifier (e.g., "Joystick_Button_35" or "Joystick_L-Axis_X").</returns>
    private string NormalizeIdentifier(string keyInfo)
    {
        // Trim any leading/trailing spaces, then replace remaining spaces with underscores
        // "Joystick Button 35" → "Joystick_Button_35"
        // "Joystick L-Axis X " → "Joystick L-Axis X" → "Joystick_L-Axis_X"
        return keyInfo.Trim().Replace(" ", "_");
    }

    /// <summary>
    /// Converts an MSFS action name to a user-friendly display name.
    /// Removes the "KEY_" prefix and capitalizes each word.
    /// </summary>
    /// <param name="actionName">The action name from the XML (e.g., "KEY_MAGNETO_START").</param>
    /// <returns>A formatted friendly name (e.g., "Magneto Start").</returns>
    private string ConvertActionToFriendly(string actionName)
    {
        if (actionName.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            actionName = actionName.Substring(keyPrefix.Length);
        }

        string[] words = actionName.Split('_', StringSplitOptions.RemoveEmptyEntries);
        string[] capitalizedWords = new string[words.Length];

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            if (word.Length > 0)
            {
                capitalizedWords[i] = char.ToUpper(word[0]) + word.Substring(1).ToLower();
            }
            else
            {
                capitalizedWords[i] = word;
            }
        }

        return string.Join(" ", capitalizedWords);
    }
}
