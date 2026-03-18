// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Msfs.ControllerVisualizer.Models;

/// <summary>
/// Provides services for loading and retrieving controller definitions from JSON configuration.
/// Loads controller metadata from the controllers.json file and provides lookup methods.
/// </summary>
public class ControllerDefinitionLoader
{
    private readonly string controllersJsonPath = "Assets/Controllers/controllers.json";

    /// <summary>
    /// Loads all supported controller definitions from the controllers.json configuration file.
    /// </summary>
    /// <returns>A list of controller definitions, or an empty list if loading fails.</returns>
    public List<ControllerDefinition> LoadSupportedControllers()
    {
        try
        {
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.controllersJsonPath);

            if (!File.Exists(jsonPath))
            {
                System.Diagnostics.Debug.WriteLine($"Controllers definition file not found: {jsonPath}");
                return new();
            }

            string json = File.ReadAllText(jsonPath);
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
            };

            ControllersData? data = JsonSerializer.Deserialize<ControllersData>(json, options);

            return data?.Controllers ?? new();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading controller definitions: {ex.Message}");
            return new();
        }
    }

    /// <summary>
    /// Gets a controller definition by its identifier (name or product ID).
    /// </summary>
    /// <param name="controllerId">The controller identifier to search for.</param>
    /// <param name="controllers">The list of controllers to search in.</param>
    /// <returns>The matching controller definition, or null if not found.</returns>
    public ControllerDefinition? GetControllerById(string controllerId, List<ControllerDefinition> controllers)
    {
        return controllers.FirstOrDefault(c =>
            c.Name.Equals(controllerId, StringComparison.OrdinalIgnoreCase) ||
            c.ProductId.Equals(controllerId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a controller definition by its name or device name.
    /// </summary>
    /// <param name="name">The controller name to search for.</param>
    /// <param name="controllers">The list of controllers to search in.</param>
    /// <returns>The matching controller definition, or null if not found.</returns>
    public ControllerDefinition? GetControllerByName(string name, List<ControllerDefinition> controllers)
    {
        return controllers.FirstOrDefault(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
            c.DeviceName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
