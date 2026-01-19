namespace Msfs.ControllerVisualizer.Models;

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
