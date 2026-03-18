// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Tests.Converters;

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Msfs.ControllerVisualizer.Converters;
using Msfs.ControllerVisualizer.Models;

/// <summary>
/// Unit tests for the <see cref="JoystickButtonMappingConverter"/> class.
/// </summary>
[TestClass]
public class JoystickButtonMappingConverterTests
{
    private JoystickButtonMappingConverter converter = null!;

    /// <summary>
    /// Initializes the converter under test.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        this.converter = new();
    }

    /// <summary>
    /// Verifies that Convert returns the friendly name for a matching button mapping.
    /// </summary>
    [TestMethod]
    public void ConvertReturnsFriendlyNameForMatchingMapping()
    {
        List<ButtonMapping> mappings =
        [
            new() { ButtonId = "Joystick_Button_1", MsfsCommand = "KEY_TOGGLE_MASTER_BATTERY", FriendlyName = "Toggle Master Battery" }
        ];
        JoystickButtonMappingConverter.SetCurrentMappings(mappings);

        object result = this.converter.Convert(null!, typeof(string), "Joystick_Button_1", CultureInfo.InvariantCulture);

        Assert.AreEqual("Toggle Master Battery", result);
    }

    /// <summary>
    /// Verifies that Convert joins multiple friendly names for the same button mapping.
    /// </summary>
    [TestMethod]
    public void ConvertReturnsCommaJoinedNamesForMultipleMappings()
    {
        List<ButtonMapping> mappings =
        [
            new() { ButtonId = "Joystick_Button_5", MsfsCommand = "KEY_FLAPS_UP", FriendlyName = "Flaps Up" },
            new() { ButtonId = "Joystick_Button_5", MsfsCommand = "KEY_GEAR_UP", FriendlyName = "Gear Up" }
        ];
        JoystickButtonMappingConverter.SetCurrentMappings(mappings);

        object result = this.converter.Convert(null!, typeof(string), "Joystick_Button_5", CultureInfo.InvariantCulture);

        Assert.AreEqual("Flaps Up, Gear Up", result);
    }

    /// <summary>
    /// Verifies that Convert returns an empty string when no mapping exists.
    /// </summary>
    [TestMethod]
    public void ConvertReturnsEmptyStringWhenNoMappingFound()
    {
        List<ButtonMapping> mappings =
        [
            new() { ButtonId = "Joystick_Button_1", MsfsCommand = "KEY_AP_MASTER", FriendlyName = "Ap Master" }
        ];
        JoystickButtonMappingConverter.SetCurrentMappings(mappings);

        object result = this.converter.Convert(null!, typeof(string), "Joystick_Button_99", CultureInfo.InvariantCulture);

        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Verifies that Convert returns an empty string when the parameter is null.
    /// </summary>
    [TestMethod]
    public void ConvertReturnsEmptyStringWhenParameterIsNull()
    {
        List<ButtonMapping> mappings =
        [
            new() { ButtonId = "Joystick_Button_1", MsfsCommand = "KEY_AP_MASTER", FriendlyName = "Ap Master" }
        ];
        JoystickButtonMappingConverter.SetCurrentMappings(mappings);

        object result = this.converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);

        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Verifies that Convert matches button identifiers without case sensitivity.
    /// </summary>
    [TestMethod]
    public void ConvertMatchesButtonIdCaseInsensitively()
    {
        List<ButtonMapping> mappings =
        [
            new() { ButtonId = "Joystick_Button_10", MsfsCommand = "KEY_STROBES_TOGGLE", FriendlyName = "Strobes Toggle" }
        ];
        JoystickButtonMappingConverter.SetCurrentMappings(mappings);

        object result = this.converter.Convert(null!, typeof(string), "joystick_button_10", CultureInfo.InvariantCulture);

        Assert.AreEqual("Strobes Toggle", result);
    }

    /// <summary>
    /// Verifies that Convert returns an empty string when mappings have not been set.
    /// </summary>
    [TestMethod]
    public void ConvertReturnsEmptyStringWhenMappingsAreNotSet()
    {
        JoystickButtonMappingConverter.SetCurrentMappings(null!);
        JoystickButtonMappingConverter freshConverter = new();

        object result = freshConverter.Convert(null!, typeof(string), "Joystick_Button_1", CultureInfo.InvariantCulture);

        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Verifies that ConvertBack throws a <see cref="NotImplementedException"/>.
    /// </summary>
    [TestMethod]
    public void ConvertBackThrowsNotImplementedException()
    {
        try
        {
            this.converter.ConvertBack("value", typeof(string), null!, CultureInfo.InvariantCulture);
            Assert.Fail("Expected NotImplementedException was not thrown");
        }
        catch (NotImplementedException)
        {
            // Expected
        }
    }
}
