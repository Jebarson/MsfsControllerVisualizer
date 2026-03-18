// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Tests.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Msfs.ControllerVisualizer.Models;

/// <summary>
/// Unit tests for the <see cref="ExportedControllerInfo"/> class.
/// </summary>
[TestClass]
public class ExportedControllerInfoTests
{
    /// <summary>
    /// Verifies that DisplayName returns the device name when it is set.
    /// </summary>
    [TestMethod]
    public void DisplayNameReturnsDeviceNameWhenSet()
    {
        ExportedControllerInfo info = new() { DeviceName = "Alpha Flight Controls" };

        Assert.AreEqual("Alpha Flight Controls", info.DisplayName);
    }

    /// <summary>
    /// Verifies that DisplayName falls back to the product identifier when the device name is empty.
    /// </summary>
    [TestMethod]
    public void DisplayNameReturnsProductIdFallbackWhenDeviceNameIsEmpty()
    {
        ExportedControllerInfo info = new() { DeviceName = string.Empty, ProductId = "6400" };

        Assert.AreEqual("Device (ID: 6400)", info.DisplayName);
    }

    /// <summary>
    /// Verifies that DisplayName returns the unknown-device fallback when both values are empty.
    /// </summary>
    [TestMethod]
    public void DisplayNameReturnsUnknownDeviceWhenBothAreEmpty()
    {
        ExportedControllerInfo info = new();

        Assert.AreEqual("Unknown Device", info.DisplayName);
    }

    /// <summary>
    /// Verifies that FileName extracts the file name from the source path.
    /// </summary>
    [TestMethod]
    public void FileNameReturnsFileNameFromSourceFilePath()
    {
        ExportedControllerInfo info = new() { SourceFilePath = @"C:\Users\Test\Mocks\Alpha Flight Controls 2024 Planes.xml" };

        Assert.AreEqual("Alpha Flight Controls 2024 Planes.xml", info.FileName);
    }

    /// <summary>
    /// Verifies that FileName returns an empty string when the source path is empty.
    /// </summary>
    [TestMethod]
    public void FileNameReturnsEmptyStringWhenSourceFilePathIsEmpty()
    {
        ExportedControllerInfo info = new();

        Assert.AreEqual(string.Empty, info.FileName);
    }
}
