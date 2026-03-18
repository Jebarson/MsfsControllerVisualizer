// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Tests.Converters;

using System.Globalization;
using Msfs.ControllerVisualizer.Converters;
using Msfs.ControllerVisualizer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Unit tests for the <see cref="JoystickButtonTruncateConverter"/> class.
/// </summary>
[TestClass]
public class JoystickButtonTruncateConverterTests
{
    private JoystickButtonTruncateConverter converter = null!;

    [TestInitialize]
    public void Setup()
    {
        this.converter = new();
    }

    [TestMethod]
    public void ConvertReturnsFullTextWhenShorterThanMaxLength()
    {
        List<ButtonMapping> mappings =
        [
            new() { ButtonId = "Joystick_Button_1", MsfsCommand = "KEY_AP_MASTER", FriendlyName = "Ap Master" }
        ];
        JoystickButtonTruncateConverter.SetCurrentMappings(mappings);

        object result = this.converter.Convert(null!, typeof(string), "Joystick_Button_1", CultureInfo.InvariantCulture);

        Assert.AreEqual("Ap Master", result);
    }

    [TestMethod]
    public void ConvertTruncatesWithEllipsisWhenExceedsMaxLength()
    {
        List<ButtonMapping> mappings =
        [
            new() { ButtonId = "Joystick_Button_1", MsfsCommand = "KEY_TOGGLE_MASTER_BATTERY", FriendlyName = "Toggle Master Battery" }
        ];
        JoystickButtonTruncateConverter.SetCurrentMappings(mappings);

        object result = this.converter.Convert(null!, typeof(string), "Joystick_Button_1", CultureInfo.InvariantCulture);

        // Default MaxLength is 15, "Toggle Master Battery" (21 chars) should truncate
        Assert.AreEqual("Toggle Master B...", result);
    }

    [TestMethod]
    public void ConvertReturnsFullTextWhenExactlyMaxLength()
    {
        this.converter.MaxLength = 9;
        List<ButtonMapping> mappings =
        [
            new() { ButtonId = "Joystick_Button_1", MsfsCommand = "KEY_AP_MASTER", FriendlyName = "Ap Master" }
        ];
        JoystickButtonTruncateConverter.SetCurrentMappings(mappings);

        object result = this.converter.Convert(null!, typeof(string), "Joystick_Button_1", CultureInfo.InvariantCulture);

        Assert.AreEqual("Ap Master", result);
    }

    [TestMethod]
    public void ConvertReturnsEmptyStringWhenNoMappingFound()
    {
        List<ButtonMapping> mappings = [];
        JoystickButtonTruncateConverter.SetCurrentMappings(mappings);

        object result = this.converter.Convert(null!, typeof(string), "Joystick_Button_99", CultureInfo.InvariantCulture);

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void ConvertRespectsCustomMaxLength()
    {
        this.converter.MaxLength = 8;
        List<ButtonMapping> mappings =
        [
            new() { ButtonId = "Joystick_Button_1", MsfsCommand = "KEY_AP_MASTER", FriendlyName = "Ap Master" }
        ];
        JoystickButtonTruncateConverter.SetCurrentMappings(mappings);

        object result = this.converter.Convert(null!, typeof(string), "Joystick_Button_1", CultureInfo.InvariantCulture);

        Assert.AreEqual("Ap Maste...", result);
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
