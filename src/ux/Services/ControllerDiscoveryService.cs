namespace Msfs.ControllerVisualizer.Services;

using System.IO;
using System.Xml.Linq;
using Msfs.ControllerVisualizer.Models;

public class ControllerDiscoveryService
{
    /// <summary>
    /// Discovers all controllers defined in XML files within the specified folder.
    /// </summary>
    /// <param name="folderPath">The path to the folder containing XML configuration files.</param>
    /// <returns>A list of discovered controller information.</returns>
    public List<ExportedControllerInfo> DiscoverControllersInFolder(string folderPath)
    {
        List<ExportedControllerInfo> controllers = new();

        try
        {
            if (!Directory.Exists(folderPath))
                return controllers;

            string[] xmlFiles = Directory.GetFiles(folderPath, "*.xml", SearchOption.TopDirectoryOnly);

            foreach (string filePath in xmlFiles)
            {
                List<ExportedControllerInfo> fileControllers = this.DiscoverControllersInFile(filePath);
                controllers.AddRange(fileControllers);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error discovering controllers in folder: {ex.Message}");
        }

        return controllers;
    }

    /// <summary>
    /// Discovers all controllers defined in a single XML export file.
    /// </summary>
    /// <param name="exportFilePath">The path to the XML export file.</param>
    /// <returns>A list of discovered controller information from the file.</returns>
    public List<ExportedControllerInfo> DiscoverControllersInFile(string exportFilePath)
    {
        List<ExportedControllerInfo> controllers = new();

        try
        {
            if (!File.Exists(exportFilePath))
                return controllers;

            // Read the entire file content
            string xmlContent = File.ReadAllText(exportFilePath);
            
            // Remove XML declaration if present (<?xml ... ?>)
            int xmlDeclEnd = xmlContent.IndexOf("?>", StringComparison.Ordinal);
            if (xmlDeclEnd > 0 && xmlContent.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
            {
                xmlContent = xmlContent.Substring(xmlDeclEnd + 2).TrimStart();
            }
            
            // Wrap content in a temporary root element to handle multiple root elements
            string wrappedXml = $"<Root>{xmlContent}</Root>";
            
            XDocument doc = XDocument.Parse(wrappedXml);
            IEnumerable<XElement> devices = doc.Descendants("Device");

            foreach (XElement device in devices)
            {
                string? deviceName = device.Attribute("DeviceName")?.Value;
                string? productId = device.Attribute("ProductID")?.Value;
                string? guid = device.Attribute("GUID")?.Value;

                if (!string.IsNullOrEmpty(deviceName) || !string.IsNullOrEmpty(productId))
                {
                    ExportedControllerInfo info = new()
                    {
                        DeviceName = deviceName ?? string.Empty,
                        ProductId = productId ?? string.Empty,
                        Guid = guid ?? string.Empty,
                        SourceFilePath = exportFilePath,
                        DeviceElement = device
                    };

                    controllers.Add(info);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error discovering controllers in file {exportFilePath}: {ex.Message}");
        }

        return controllers;
    }

    /// <summary>
    /// Attempts to match an exported controller with a supported controller definition.
    /// </summary>
    /// <param name="exportedInfo">The exported controller information to match.</param>
    /// <param name="supportedControllers">The list of supported controller definitions.</param>
    /// <returns>The matching controller definition, or null if no match is found.</returns>
    public ControllerDefinition? MatchController(ExportedControllerInfo exportedInfo, List<ControllerDefinition> supportedControllers)
    {
        foreach (ControllerDefinition supported in supportedControllers)
        {
            if (this.IsMatch(exportedInfo, supported))
            {
                return supported;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets all supported controllers from the specified folder, consolidating multiple profiles for the same device.
    /// </summary>
    /// <param name="folderPath">The path to the folder containing XML configuration files.</param>
    /// <param name="supportedControllers">The list of supported controller definitions.</param>
    /// <returns>A consolidated list of supported controllers found in the folder.</returns>
    public List<ExportedControllerInfo> GetSupportedControllers(string folderPath, List<ControllerDefinition> supportedControllers)
    {
        List<ExportedControllerInfo> allControllers = this.DiscoverControllersInFolder(folderPath);
        List<ExportedControllerInfo> supported = new();

        foreach (ExportedControllerInfo controller in allControllers)
        {
            if (this.MatchController(controller, supportedControllers) != null)
            {
                supported.Add(controller);
            }
        }

        // Group by DeviceName to consolidate multiple profiles for the same device
        List<ExportedControllerInfo> consolidated = new();
        Dictionary<string, List<ExportedControllerInfo>> grouped = supported
            .GroupBy(c => c.DeviceName)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (KeyValuePair<string, List<ExportedControllerInfo>> group in grouped)
        {
            if (group.Value.Count == 1)
            {
                // Only one profile for this device
                consolidated.Add(group.Value[0]);
            }
            else
            {
                // Multiple profiles - merge them
                ExportedControllerInfo merged = this.MergeControllerProfiles(group.Value);
                consolidated.Add(merged);
            }
        }

        return consolidated;
    }

    /// <summary>
    /// Determines whether an exported controller matches a supported controller definition.
    /// </summary>
    /// <param name="exportedInfo">The exported controller information.</param>
    /// <param name="supported">The supported controller definition.</param>
    /// <returns>True if the controllers match; otherwise, false.</returns>
    private bool IsMatch(ExportedControllerInfo exportedInfo, ControllerDefinition supported)
    {
        // Match by ProductId first (most specific)
        if (!string.IsNullOrEmpty(exportedInfo.ProductId) &&
            !string.IsNullOrEmpty(supported.ProductId) &&
            exportedInfo.ProductId.Equals(supported.ProductId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Then match by DeviceName (less specific)
        if (!string.IsNullOrEmpty(exportedInfo.DeviceName) &&
            !string.IsNullOrEmpty(supported.DeviceName) &&
            exportedInfo.DeviceName.Contains(supported.DeviceName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Merges multiple controller profiles for the same device into a single consolidated profile.
    /// </summary>
    /// <param name="profiles">The list of profiles to merge.</param>
    /// <returns>A merged controller profile containing all contexts from the input profiles.</returns>
    private ExportedControllerInfo MergeControllerProfiles(List<ExportedControllerInfo> profiles)
    {
        // Use the first profile as the base
        ExportedControllerInfo baseProfile = profiles[0];
        
        // Create a merged XElement by combining all Device elements
        XElement mergedDevice = new(baseProfile.DeviceElement!);
        
        // Merge contexts from all profiles
        for (int i = 1; i < profiles.Count; i++)
        {
            XElement? sourceDevice = profiles[i].DeviceElement;
            if (sourceDevice != null)
            {
                IEnumerable<XElement> contexts = sourceDevice.Elements("Context");
                foreach (XElement context in contexts)
                {
                    mergedDevice.Add(new XElement(context));
                }
            }
        }

        return new()
        {
            DeviceName = baseProfile.DeviceName,
            ProductId = baseProfile.ProductId,
            Guid = baseProfile.Guid,
            SourceFilePath = string.Join("; ", profiles.Select(p => Path.GetFileName(p.SourceFilePath))),
            DeviceElement = mergedDevice
        };
    }
}
