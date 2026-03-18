// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Tests.Xaml;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Msfs.ControllerVisualizer.Converters;
using Msfs.ControllerVisualizer.Models;
using Msfs.ControllerVisualizer.Services;

/// <summary>
/// Unit tests for verifying XAML loading and controller visual rendering.
/// </summary>
[TestClass]
public class XamlLoadingTests
{
    private static readonly string AssetsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Controllers");
    private static readonly string MocksDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks");

    /// <summary>
    /// Verifies that each controller visual XAML file loads as a <see cref="UserControl"/>.
    /// </summary>
    /// <param name="xamlFile">The controller visual XAML file name.</param>
    [TestMethod]
    [DataRow("HoneycombAlpha.xaml")]
    [DataRow("HoneycombBravo.xaml")]
    [DataRow("SaitekProFlightRudderPedals2024.xaml")]
    [DataRow("MfdCougar2024Planes.xaml")]
    public void EachVisualFileLoadsAsUserControl(string xamlFile)
    {
        RunOnStaThread(() =>
        {
            string xamlPath = Path.Combine(AssetsDir, xamlFile);
            Assert.IsTrue(File.Exists(xamlPath), $"XAML file should exist: {xamlPath}");

            // Set empty mappings so converters don't fail
            JoystickButtonMappingConverter.SetCurrentMappings([]);
            JoystickButtonTruncateConverter.SetCurrentMappings([]);

            using FileStream stream = File.OpenRead(xamlPath);
            ParserContext context = new()
            {
                BaseUri = new Uri(xamlPath, UriKind.Absolute),
            };
            context.XmlnsDictionary.Add(string.Empty, "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            context.XmlnsDictionary.Add("converters", "clr-namespace:Msfs.ControllerVisualizer.Converters;assembly=Msfs.ControllerVisualizer");

            object loaded = XamlReader.Load(stream, context);

            Assert.IsInstanceOfType<UserControl>(loaded, $"{xamlFile} should load as a UserControl");
        });
    }

    /// <summary>
    /// Verifies that controller visuals load successfully when populated with mock controller data.
    /// </summary>
    /// <param name="mockXmlFile">The mock XML file containing controller bindings.</param>
    /// <param name="expectedXamlFile">The expected visual XAML file.</param>
    [TestMethod]
    [DataRow("Alpha Flight Controls 2024 Planes.xml", "HoneycombAlpha.xaml")]
    [DataRow("Bravo Throttle Quadrant 2024 Planes.xml", "HoneycombBravo.xaml")]
    [DataRow("Saitek Pro Flight Rudder Pedals 2024 Planes.xml", "SaitekProFlightRudderPedals2024.xaml")]
    [DataRow("MFD COUGAR Left 2024 Planes.xml", "MfdCougar2024Planes.xaml")]
    [DataRow("MFD COUGAR Right 2024 Planes.xml", "MfdCougar2024Planes.xaml")]
    public void XamlLoadsSuccessfullyWithMockControllerData(string mockXmlFile, string expectedXamlFile)
    {
        RunOnStaThread(() =>
        {
            // Step 1: Discover the controller from the mock XML
            ControllerDiscoveryService discoveryService = new();
            string mockPath = Path.Combine(MocksDir, mockXmlFile);
            List<ExportedControllerInfo> discovered = discoveryService.DiscoverControllersInFile(mockPath);
            Assert.IsTrue(discovered.Count > 0, $"Should discover at least one controller in {mockXmlFile}");

            ExportedControllerInfo exportedInfo = discovered[0];

            // Step 2: Match to a supported controller definition
            ControllerDefinitionLoader definitionLoader = new();
            List<ControllerDefinition> supported = definitionLoader.LoadSupportedControllers();
            ControllerDefinition? matchedDef = discoveryService.MatchController(exportedInfo, supported);
            Assert.IsNotNull(matchedDef, $"Mock '{mockXmlFile}' should match a supported controller definition");
            Assert.AreEqual(expectedXamlFile, matchedDef.VisualFile, $"Mock '{mockXmlFile}' should map to '{expectedXamlFile}'");

            // Step 3: Map buttons from the discovered device XML
            ControllerButtonMapper buttonMapper = new();
            List<ButtonMapping> mappings = buttonMapper.MapButtons(exportedInfo.DeviceElement!, matchedDef);

            // Step 4: Set converter mappings and load the XAML visual
            JoystickButtonMappingConverter.SetCurrentMappings(mappings);
            JoystickButtonTruncateConverter.SetCurrentMappings(mappings);

            string xamlPath = Path.Combine(AssetsDir, matchedDef.VisualFile);
            Assert.IsTrue(File.Exists(xamlPath), $"XAML file should exist: {xamlPath}");

            using FileStream stream = File.OpenRead(xamlPath);
            ParserContext context = new()
            {
                BaseUri = new Uri(xamlPath, UriKind.Absolute),
            };
            context.XmlnsDictionary.Add(string.Empty, "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            context.XmlnsDictionary.Add("converters", "clr-namespace:Msfs.ControllerVisualizer.Converters;assembly=Msfs.ControllerVisualizer");

            object loaded = XamlReader.Load(stream, context);

            Assert.IsInstanceOfType<UserControl>(loaded, $"{expectedXamlFile} should load as a UserControl with mock data from {mockXmlFile}");
        });
    }

    /// <summary>
    /// Verifies that mock XML bindings produce valid mappings for XAML converter parameters.
    /// </summary>
    /// <param name="mockXmlFile">The mock XML file containing controller bindings.</param>
    [TestMethod]
    [DataRow("MFD COUGAR Right 2024 Planes.xml")]
    public void MockXmlWithBindingsProducesMappingsForXamlConverterParameters(string mockXmlFile)
    {
        // Verify that button mappings from mock XML match converter parameters referenced in the XAML
        ControllerDiscoveryService discoveryService = new();
        string mockPath = Path.Combine(MocksDir, mockXmlFile);
        List<ExportedControllerInfo> discovered = discoveryService.DiscoverControllersInFile(mockPath);
        ExportedControllerInfo exportedInfo = discovered[0];

        ControllerDefinitionLoader definitionLoader = new();
        List<ControllerDefinition> supported = definitionLoader.LoadSupportedControllers();
        ControllerDefinition? matchedDef = discoveryService.MatchController(exportedInfo, supported);
        Assert.IsNotNull(matchedDef);

        ControllerButtonMapper buttonMapper = new();
        List<ButtonMapping> mappings = buttonMapper.MapButtons(exportedInfo.DeviceElement!, matchedDef);

        // The MFD Right mock has bound buttons (e.g., Button 24 -> Anti Ice Toggle)
        Assert.IsTrue(mappings.Count > 0, "MFD Right mock should produce at least one button mapping");

        // Verify each mapping has valid data
        foreach (ButtonMapping mapping in mappings)
        {
            Assert.IsFalse(string.IsNullOrEmpty(mapping.ButtonId), "ButtonId should not be empty");
            Assert.IsFalse(string.IsNullOrEmpty(mapping.MsfsCommand), "MsfsCommand should not be empty");
            Assert.IsFalse(string.IsNullOrEmpty(mapping.FriendlyName), "FriendlyName should not be empty");
        }
    }

    /// <summary>
    /// Verifies that the controller visual converter returns null when all values are null.
    /// </summary>
    [TestMethod]
    public void ControllerVisualConverterReturnsNullForNullValues()
    {
        ControllerVisualConverter converter = new();

        object? result = converter.Convert([null!, null!], typeof(object), null!, System.Globalization.CultureInfo.InvariantCulture);

        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that the controller visual converter returns null when insufficient values are provided.
    /// </summary>
    [TestMethod]
    public void ControllerVisualConverterReturnsNullForInsufficientValues()
    {
        ControllerVisualConverter converter = new();

        object? result = converter.Convert([new object()], typeof(object), null!, System.Globalization.CultureInfo.InvariantCulture);

        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that the controller visual converter returns null when the visual file is missing.
    /// </summary>
    [TestMethod]
    public void ControllerVisualConverterReturnsNullForMissingVisualFile()
    {
        RunOnStaThread(() =>
        {
            ControllerVisualConverter converter = new();
            ControllerDefinition definition = new()
            {
                Name = "Nonexistent",
                DeviceName = "Nonexistent",
                ProductId = "0000",
                VisualFile = "NonexistentFile.xaml",
            };
            List<ButtonMapping> mappings = [];

            object? result = converter.Convert([definition, mappings], typeof(object), null!, System.Globalization.CultureInfo.InvariantCulture);

            Assert.IsNull(result);
        });
    }

    /// <summary>
    /// Verifies that the controller visual converter throws <see cref="NotImplementedException"/> from ConvertBack.
    /// </summary>
    [TestMethod]
    public void ControllerVisualConverterConvertBackThrowsNotImplementedException()
    {
        ControllerVisualConverter converter = new();

        try
        {
            converter.ConvertBack(new object(), [typeof(object)], null!, System.Globalization.CultureInfo.InvariantCulture);
            Assert.Fail("Expected NotImplementedException was not thrown");
        }
        catch (NotImplementedException)
        {
            // Expected
        }
    }

    private static void RunOnStaThread(Action action)
    {
        ExceptionDispatchInfo? capturedException = null;

        Thread thread = new(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                capturedException = ExceptionDispatchInfo.Capture(ex);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        capturedException?.Throw();
    }
}
