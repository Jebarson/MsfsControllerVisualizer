namespace Msfs.ControllerVisualizer.Models;

/// <summary>
/// Represents a button definition on a controller.
/// </summary>
public class ButtonDefinition
{
    /// <summary>
    /// Gets or sets the numeric identifier of the button.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the button.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the visual identifier used in the XAML representation.
    /// </summary>
    public string VisualId { get; set; } = string.Empty;
}
