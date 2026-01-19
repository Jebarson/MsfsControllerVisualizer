namespace Msfs.ControllerVisualizer.Models;

/// <summary>
/// Root data structure for MSFS commands configuration file.
/// </summary>
public class MsfsCommandsData
{
    /// <summary>
    /// Gets or sets the list of MSFS command mappings.
    /// </summary>
    public List<MsfsCommandMapping> Commands { get; set; } = new();
}
