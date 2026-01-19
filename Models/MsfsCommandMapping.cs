namespace Msfs.ControllerVisualizer.Models;

/// <summary>
/// Represents a mapping between an MSFS command and its friendly display information.
/// </summary>
public class MsfsCommandMapping
{
    /// <summary>
    /// Gets or sets the MSFS command key (e.g., "KEY_MAGNETO_START").
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user-friendly name of the command.
    /// </summary>
    public string FriendlyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category this command belongs to.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of what this command does.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
