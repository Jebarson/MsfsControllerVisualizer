// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Tests.Services;

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Msfs.ControllerVisualizer.Models;
using Msfs.ControllerVisualizer.Services;

/// <summary>
/// Unit tests for the <see cref="ControllerDefinitionLoader"/> class.
/// </summary>
[TestClass]
public class ControllerDefinitionLoaderTests
{
    private ControllerDefinitionLoader loader = null!;

    /// <summary>
    /// Initializes the loader under test.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        this.loader = new();
    }

    /// <summary>
    /// Verifies that the loader returns the expected number of supported controller definitions.
    /// </summary>
    [TestMethod]
    public void LoadSupportedControllersReturnsFiveDefinitions()
    {
        List<ControllerDefinition> controllers = this.loader.LoadSupportedControllers();

        Assert.AreEqual(5, controllers.Count);
    }

    /// <summary>
    /// Verifies that loaded controller definitions contain non-empty required fields.
    /// </summary>
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

    /// <summary>
    /// Verifies that all loaded controller definitions reference visual files that exist.
    /// </summary>
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

    /// <summary>
    /// Verifies that GetControllerById finds a controller by product identifier.
    /// </summary>
    /// <param name="expectedName">The expected controller name.</param>
    /// <param name="productId">The product identifier to search for.</param>
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

    /// <summary>
    /// Verifies that GetControllerById also finds a controller when passed its name.
    /// </summary>
    /// <param name="name">The controller name to search for.</param>
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

    /// <summary>
    /// Verifies that GetControllerByName finds a controller by device name.
    /// </summary>
    /// <param name="deviceName">The device name to search for.</param>
    /// <param name="expectedName">The expected controller name.</param>
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

    /// <summary>
    /// Verifies that GetControllerById returns null for an unknown identifier.
    /// </summary>
    [TestMethod]
    public void GetControllerByIdReturnsNullForUnknownId()
    {
        List<ControllerDefinition> controllers = this.loader.LoadSupportedControllers();

        ControllerDefinition? found = this.loader.GetControllerById("99999", controllers);

        Assert.IsNull(found);
    }

    /// <summary>
    /// Verifies that GetControllerByName returns null for an unknown device name.
    /// </summary>
    [TestMethod]
    public void GetControllerByNameReturnsNullForUnknownName()
    {
        List<ControllerDefinition> controllers = this.loader.LoadSupportedControllers();

        ControllerDefinition? found = this.loader.GetControllerByName("Unknown Device", controllers);

        Assert.IsNull(found);
    }
}
