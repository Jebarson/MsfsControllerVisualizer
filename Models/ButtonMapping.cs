namespace Msfs.ControllerVisualizer.Models;

/// <summary>
/// Represents a mapping between a controller button and an MSFS command action.
/// </summary>
public class ButtonMapping
{
    /// <summary>
    /// Gets or sets the button identifier (e.g., "Joystick_Button_35" or "Joystick_Pov_Up").
    /// </summary>
    public string ButtonId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MSFS command key (e.g., "KEY_MAGNETO_START").
    /// </summary>
    public string MsfsCommand { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user-friendly name of the mapped action.
    /// </summary>
    public string FriendlyName { get; set; } = string.Empty;
}
