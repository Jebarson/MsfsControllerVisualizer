// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Tests.Converters;

using System.Globalization;
using System.Windows;
using Msfs.ControllerVisualizer.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Unit tests for the <see cref="NullToVisibilityConverter"/> class.
/// </summary>
[TestClass]
public class NullToVisibilityConverterTests
{
    private NullToVisibilityConverter converter = null!;

    [TestInitialize]
    public void Setup()
    {
        this.converter = new();
    }

    [TestMethod]
    public void ConvertReturnsCollapsedForNullValue()
    {
        object result = this.converter.Convert(null!, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.AreEqual(Visibility.Collapsed, result);
    }

    [TestMethod]
    public void ConvertReturnsVisibleForNonNullValue()
    {
        object result = this.converter.Convert("something", typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.AreEqual(Visibility.Visible, result);
    }

    [TestMethod]
    public void ConvertReturnsVisibleForNullValueWhenInverted()
    {
        object result = this.converter.Convert(null!, typeof(Visibility), "Inverted", CultureInfo.InvariantCulture);

        Assert.AreEqual(Visibility.Visible, result);
    }

    [TestMethod]
    public void ConvertReturnsCollapsedForNonNullValueWhenInverted()
    {
        object result = this.converter.Convert("something", typeof(Visibility), "Inverted", CultureInfo.InvariantCulture);

        Assert.AreEqual(Visibility.Collapsed, result);
    }

    [TestMethod]
    public void ConvertBackThrowsNotImplementedException()
    {
        try
        {
            this.converter.ConvertBack(Visibility.Visible, typeof(object), null!, CultureInfo.InvariantCulture);
            Assert.Fail("Expected NotImplementedException was not thrown");
        }
        catch (NotImplementedException)
        {
            // Expected
        }
    }
}
