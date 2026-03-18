// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Tests.Models;

using Msfs.ControllerVisualizer.Models;

/// <summary>
/// Unit tests for the <see cref="ExportedControllerInfo"/> class.
/// </summary>
[TestClass]
public class ExportedControllerInfoTests
{
    [TestMethod]
    public void DisplayNameReturnsDeviceNameWhenSet()
    {
        ExportedControllerInfo info = new() { DeviceName = "Alpha Flight Controls" };

        Assert.AreEqual("Alpha Flight Controls", info.DisplayName);
    }

    [TestMethod]
    public void DisplayNameReturnsProductIdFallbackWhenDeviceNameIsEmpty()
    {
        ExportedControllerInfo info = new() { DeviceName = "", ProductId = "6400" };

        Assert.AreEqual("Device (ID: 6400)", info.DisplayName);
    }

    [TestMethod]
    public void DisplayNameReturnsUnknownDeviceWhenBothAreEmpty()
    {
        ExportedControllerInfo info = new();

        Assert.AreEqual("Unknown Device", info.DisplayName);
    }

    [TestMethod]
    public void FileNameReturnsFileNameFromSourceFilePath()
    {
        ExportedControllerInfo info = new() { SourceFilePath = @"C:\Users\Test\Mocks\Alpha Flight Controls 2024 Planes.xml" };

        Assert.AreEqual("Alpha Flight Controls 2024 Planes.xml", info.FileName);
    }

    [TestMethod]
    public void FileNameReturnsEmptyStringWhenSourceFilePathIsEmpty()
    {
        ExportedControllerInfo info = new();

        Assert.AreEqual(string.Empty, info.FileName);
    }
}
