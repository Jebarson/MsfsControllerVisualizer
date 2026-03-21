// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Tests.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Msfs.ControllerVisualizer.Models;
using Msfs.ControllerVisualizer.Services;

/// <summary>
/// Unit tests for the <see cref="ControllerDiscoveryService"/> class.
/// </summary>
[TestClass]
public class ControllerDiscoveryServiceTests
{
    private ControllerDiscoveryService service = null!;
    private string mocksFolder = null!;

    /// <summary>
    /// Initializes the discovery service and mock folder path used by the tests.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        this.service = new();
        this.mocksFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks");
    }

    /// <summary>
    /// Verifies that a valid XML file produces a single discovered device.
    /// </summary>
    [TestMethod]
    public void DiscoverControllersInFileReturnsSingleDeviceFromValidXml()
    {
        string filePath = Path.Combine(this.mocksFolder, "Saitek Pro Flight Rudder Pedals 2024 Planes.xml");

        List<ExportedControllerInfo> controllers = this.service.DiscoverControllersInFile(filePath);

        Assert.AreEqual(1, controllers.Count);
        Assert.AreEqual("Saitek Pro Flight Rudder Pedals", controllers[0].DeviceName);
        Assert.AreEqual("1891", controllers[0].ProductId);
    }

    /// <summary>
    /// Verifies that discovering controllers in a missing file returns an empty result.
    /// </summary>
    [TestMethod]
    public void DiscoverControllersInFileReturnsEmptyForNonExistentFile()
    {
        List<ExportedControllerInfo> controllers = this.service.DiscoverControllersInFile("nonexistent.xml");

        Assert.AreEqual(0, controllers.Count);
    }

    /// <summary>
    /// Verifies that discovering controllers in the mocks folder returns all devices.
    /// </summary>
    [TestMethod]
    public void DiscoverControllersInFolderReturnsAllDevicesFromMocksFolder()
    {
        List<ExportedControllerInfo> controllers = this.service.DiscoverControllersInFolder(this.mocksFolder);

        Assert.IsTrue(controllers.Count >= 6, $"Expected at least 6 controllers but found {controllers.Count}");
    }

    /// <summary>
    /// Verifies that discovering controllers in a missing folder returns an empty result.
    /// </summary>
    [TestMethod]
    public void DiscoverControllersInFolderReturnsEmptyForNonExistentFolder()
    {
        List<ExportedControllerInfo> controllers = this.service.DiscoverControllersInFolder("nonexistent_folder");

        Assert.AreEqual(0, controllers.Count);
    }

    /// <summary>
    /// Verifies that discovery extracts the expected device information from a file.
    /// </summary>
    /// <param name="fileName">The mock XML file name.</param>
    /// <param name="expectedDeviceName">The expected device name.</param>
    /// <param name="expectedProductId">The expected product identifier.</param>
    [DataTestMethod]
    [DataRow("Saitek Pro Flight Rudder Pedals 2024 Planes.xml", "Saitek Pro Flight Rudder Pedals", "1891")]
    [DataRow("Alpha Flight Controls 2024 Planes.xml", "Alpha Flight Controls", "6400")]
    [DataRow("Bravo Throttle Quadrant 2024 Planes.xml", "Bravo Throttle Quadrant", "6401")]
    public void DiscoverControllersInFileExtractsCorrectDeviceInfo(string fileName, string expectedDeviceName, string expectedProductId)
    {
        string filePath = Path.Combine(this.mocksFolder, fileName);

        List<ExportedControllerInfo> controllers = this.service.DiscoverControllersInFile(filePath);

        Assert.AreEqual(1, controllers.Count);
        Assert.AreEqual(expectedDeviceName, controllers[0].DeviceName);
        Assert.AreEqual(expectedProductId, controllers[0].ProductId);
    }

    /// <summary>
    /// Verifies that controller matching succeeds when the product identifier matches.
    /// </summary>
    [TestMethod]
    public void MatchControllerMatchesByProductId()
    {
        List<ControllerDefinition> supported =
        [
            new() { Name = "Test Controller", DeviceName = "Test Device", ProductId = "1891", VisualFile = "Test.xaml" }
        ];
        ExportedControllerInfo exported = new() { DeviceName = "Saitek Pro Flight Rudder Pedals", ProductId = "1891" };

        ControllerDefinition? match = this.service.MatchController(exported, supported);

        Assert.IsNotNull(match);
        Assert.AreEqual("1891", match.ProductId);
    }

    /// <summary>
    /// Verifies that controller matching succeeds when the device name matches.
    /// </summary>
    [TestMethod]
    public void MatchControllerMatchesByDeviceName()
    {
        List<ControllerDefinition> supported =
        [
            new() { Name = "Test", DeviceName = "Alpha Flight Controls", ProductId = "9999", VisualFile = "Test.xaml" }
        ];
        ExportedControllerInfo exported = new() { DeviceName = "Alpha Flight Controls", ProductId = "0000" };

        ControllerDefinition? match = this.service.MatchController(exported, supported);

        Assert.IsNotNull(match);
        Assert.AreEqual("Alpha Flight Controls", match.DeviceName);
    }

    /// <summary>
    /// Verifies that controller matching returns null when no supported definition matches.
    /// </summary>
    [TestMethod]
    public void MatchControllerReturnsNullWhenNoMatch()
    {
        List<ControllerDefinition> supported =
        [
            new() { Name = "Test", DeviceName = "Some Other Device", ProductId = "9999", VisualFile = "Test.xaml" }
        ];
        ExportedControllerInfo exported = new() { DeviceName = "Unknown Controller", ProductId = "0000" };

        ControllerDefinition? match = this.service.MatchController(exported, supported);

        Assert.IsNull(match);
    }

    /// <summary>
    /// Verifies that merged controller profiles consolidate data for the same device.
    /// </summary>
    [TestMethod]
    public void MergeControllerProfilesConsolidatesMultipleProfilesForSameDevice()
    {
        ControllerDefinitionLoader loader = new();
        List<ControllerDefinition> supported = loader.LoadSupportedControllers();

        List<ExportedControllerInfo> consolidated = this.service.GetSupportedControllers(this.mocksFolder, supported);

        // Alpha Flight Controls has two files (Planes and Transversal) but same DeviceName
        // So they should be merged into one entry
        int alphaCount = consolidated.Count(c => c.DeviceName == "Alpha Flight Controls");
        Assert.AreEqual(1, alphaCount, "Alpha Flight Controls should be consolidated into a single entry");

        // The merged entry should have contexts from both profiles
        ExportedControllerInfo alpha = consolidated.First(c => c.DeviceName == "Alpha Flight Controls");
        Assert.IsNotNull(alpha.DeviceElement);

        List<XElement> contexts = alpha.DeviceElement.Elements("Context").ToList();
        Assert.IsTrue(contexts.Count > 1, "Merged profile should have contexts from multiple source files");
    }

    /// <summary>
    /// Verifies that GetSupportedControllers returns only controllers that match supported definitions.
    /// </summary>
    [TestMethod]
    public void GetSupportedControllersReturnsOnlyMatchingControllers()
    {
        ControllerDefinitionLoader loader = new();
        List<ControllerDefinition> supported = loader.LoadSupportedControllers();

        List<ExportedControllerInfo> result = this.service.GetSupportedControllers(this.mocksFolder, supported);

        Assert.IsTrue(result.Count > 0, "Should find at least one supported controller");
        foreach (ExportedControllerInfo controller in result)
        {
            ControllerDefinition? match = this.service.MatchController(controller, supported);
            Assert.IsNotNull(match, $"Controller '{controller.DeviceName}' should match a supported definition");
        }
    }
}
