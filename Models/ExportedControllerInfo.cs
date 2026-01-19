namespace Msfs.ControllerVisualizer.Models;

using System.IO;
using System.Xml.Linq;

/// <summary>
/// Represents information about a controller discovered from an exported XML configuration file.
/// </summary>
public class ExportedControllerInfo
{
    /// <summary>
    /// Gets or sets the name of the device as specified in the XML configuration.
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product identifier of the device.
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the globally unique identifier (GUID) of the device.
    /// </summary>
    public string Guid { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file path where this controller configuration was found.
    /// May contain multiple file names separated by semicolons if profiles were merged.
    /// </summary>
    public string SourceFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the XML element containing the device configuration data.
    /// </summary>
    public XElement? DeviceElement { get; set; }

    /// <summary>
    /// Gets a user-friendly display name for the controller.
    /// This consolidates multiple profiles for the same device.
    /// </summary>
    public string DisplayName
    {
        get
        {
            // Show only DeviceName - this will consolidate multiple profiles for the same device
            if (!string.IsNullOrEmpty(this.DeviceName))
            {
                return this.DeviceName;
            }

            if (!string.IsNullOrEmpty(this.ProductId))
            {
                return $"Device (ID: {this.ProductId})";
            }

            return "Unknown Device";
        }
    }

    /// <summary>
    /// Gets the file name (without path) from the source file path.
    /// </summary>
    public string FileName => !string.IsNullOrEmpty(this.SourceFilePath) 
        ? Path.GetFileName(this.SourceFilePath) 
        : string.Empty;
}
