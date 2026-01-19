namespace Msfs.ControllerVisualizer.Models;

/// <summary>
/// Represents the definition of a supported controller device.
/// </summary>
public class ControllerDefinition
{
    /// <summary>
    /// Gets or sets the display name of the controller.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the device name as reported by the system.
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product identifier of the controller.
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the filename of the XAML visual representation file.
    /// </summary>
    public string VisualFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of button definitions for this controller.
    /// </summary>
    public List<ButtonDefinition> Buttons { get; set; } = new();
}
