// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Tests.Services;

using System.IO;
using Msfs.ControllerVisualizer.Models;
using Msfs.ControllerVisualizer.Services;

/// <summary>
/// Unit tests for the <see cref="ControllerDefinitionLoader"/> class.
/// </summary>
[TestClass]
public class ControllerDefinitionLoaderTests
{
    private ControllerDefinitionLoader loader = null!;

    [TestInitialize]
    public void Setup()
    {
        this.loader = new();
    }

    [TestMethod]
    public void LoadSupportedControllersReturnsFiveDefinitions()
    {
        List<ControllerDefinition> controllers = this.loader.LoadSupportedControllers();

        Assert.AreEqual(5, controllers.Count);
    }

    [TestMethod]
    public void LoadSupportedControllersReturnsDefinitionsWithNonEmptyFields()
    {
        List<ControllerDefinition> controllers = this.loader.LoadSupportedControllers();

        foreach (ControllerDefinition controller in controllers)
        {
            Assert.IsFalse(string.IsNullOrEmpty(controller.Name), $"Name should not be empty for controller with ProductId '{controller.ProductId}'");
            Assert.IsFalse(string.IsNullOrEmpty(controller.DeviceName), $"DeviceName should not be empty for '{controller.Name}'");
            Assert.IsFalse(string.IsNullOrEmpty(controller.ProductId), $"ProductId should not be empty for '{controller.Name}'");
            Assert.IsFalse(string.IsNullOrEmpty(controller.VisualFile), $"VisualFile should not be empty for '{controller.Name}'");
        }
    }

    [TestMethod]
    public void LoadSupportedControllersReturnsDefinitionsWithExistingVisualFiles()
    {
        List<ControllerDefinition> controllers = this.loader.LoadSupportedControllers();
        string assetsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Controllers");

        foreach (ControllerDefinition controller in controllers)
        {
            string visualPath = Path.Combine(assetsDir, controller.VisualFile);
            Assert.IsTrue(File.Exists(visualPath), $"Visual file '{controller.VisualFile}' for '{controller.Name}' should exist at '{visualPath}'");
        }
    }

    [TestMethod]
    [DataRow("Honeycomb Alpha Flight Control", "6400")]
    [DataRow("Honeycomb Bravo Throttle Quadrant", "6401")]
    [DataRow("Saitek Pro Flight Rudder Pedals 2024 Planes", "1891")]
    [DataRow("MFD COUGAR Left 2024 Planes", "45905")]
    [DataRow("MFD COUGAR Right 2024 Planes", "45906")]
    public void GetControllerByIdFindsControllerByProductId(string expectedName, string productId)
    {
        List<ControllerDefinition> controllers = this.loader.LoadSupportedControllers();

        ControllerDefinition? found = this.loader.GetControllerById(productId, controllers);

        Assert.IsNotNull(found, $"Should find controller with ProductId '{productId}'");
        Assert.AreEqual(expectedName, found.Name);
    }

    [TestMethod]
    [DataRow("Honeycomb Alpha Flight Control")]
    [DataRow("Honeycomb Bravo Throttle Quadrant")]
    [DataRow("Saitek Pro Flight Rudder Pedals 2024 Planes")]
    [DataRow("MFD COUGAR Left 2024 Planes")]
    [DataRow("MFD COUGAR Right 2024 Planes")]
    public void GetControllerByIdFindsControllerByName(string name)
    {
        List<ControllerDefinition> controllers = this.loader.LoadSupportedControllers();

        ControllerDefinition? found = this.loader.GetControllerById(name, controllers);

        Assert.IsNotNull(found, $"Should find controller with Name '{name}'");
        Assert.AreEqual(name, found.Name);
    }

    [TestMethod]
    [DataRow("Alpha Flight Controls", "Honeycomb Alpha Flight Control")]
    [DataRow("Bravo Throttle", "Honeycomb Bravo Throttle Quadrant")]
    [DataRow("Saitek Pro Flight Rudder Pedals", "Saitek Pro Flight Rudder Pedals 2024 Planes")]
    [DataRow("F16 MFD 1", "MFD COUGAR Left 2024 Planes")]
    [DataRow("F16 MFD 2", "MFD COUGAR Right 2024 Planes")]
    public void GetControllerByNameFindsControllerByDeviceName(string deviceName, string expectedName)
    {
        List<ControllerDefinition> controllers = this.loader.LoadSupportedControllers();

        ControllerDefinition? found = this.loader.GetControllerByName(deviceName, controllers);

        Assert.IsNotNull(found, $"Should find controller with DeviceName '{deviceName}'");
        Assert.AreEqual(expectedName, found.Name);
    }

    [TestMethod]
    public void GetControllerByIdReturnsNullForUnknownId()
    {
        List<ControllerDefinition> controllers = this.loader.LoadSupportedControllers();

        ControllerDefinition? found = this.loader.GetControllerById("99999", controllers);

        Assert.IsNull(found);
    }

    [TestMethod]
    public void GetControllerByNameReturnsNullForUnknownName()
    {
        List<ControllerDefinition> controllers = this.loader.LoadSupportedControllers();

        ControllerDefinition? found = this.loader.GetControllerByName("Unknown Device", controllers);

        Assert.IsNull(found);
    }
}
