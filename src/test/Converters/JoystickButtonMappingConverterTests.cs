// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Tests.Converters;

using System.Globalization;
using Msfs.ControllerVisualizer.Converters;
using Msfs.ControllerVisualizer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Unit tests for the <see cref="JoystickButtonMappingConverter"/> class.
/// </summary>
[TestClass]
public class JoystickButtonMappingConverterTests
{
    private JoystickButtonMappingConverter converter = null!;

    [TestInitialize]
    public void Setup()
    {
        this.converter = new();
    }

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

    [TestMethod]
    public void ConvertReturnsEmptyStringWhenMappingsAreNotSet()
    {
        JoystickButtonMappingConverter.SetCurrentMappings(null!);
        JoystickButtonMappingConverter freshConverter = new();

        object result = freshConverter.Convert(null!, typeof(string), "Joystick_Button_1", CultureInfo.InvariantCulture);

        Assert.AreEqual(string.Empty, result);
    }

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
