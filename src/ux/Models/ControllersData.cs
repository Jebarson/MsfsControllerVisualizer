// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Models;

using System.Collections.Generic;

/// <summary>
/// Root data structure for controllers configuration file.
/// </summary>
public class ControllersData
{
    /// <summary>
    /// Gets or sets the list of controller definitions.
    /// </summary>
    public List<ControllerDefinition> Controllers { get; set; } = new();
}
