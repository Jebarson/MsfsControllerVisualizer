// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Tests.Converters;

using System;
using System.Globalization;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Msfs.ControllerVisualizer.Converters;

/// <summary>
/// Unit tests for the <see cref="NullToVisibilityConverter"/> class.
/// </summary>
[TestClass]
public class NullToVisibilityConverterTests
{
    private NullToVisibilityConverter converter = null!;

    /// <summary>
    /// Initializes the converter under test.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        this.converter = new();
    }

    /// <summary>
    /// Verifies that Convert returns <see cref="Visibility.Collapsed"/> for a null value.
    /// </summary>
    [TestMethod]
    public void ConvertReturnsCollapsedForNullValue()
    {
        object result = this.converter.Convert(null!, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.AreEqual(Visibility.Collapsed, result);
    }

    /// <summary>
    /// Verifies that Convert returns <see cref="Visibility.Visible"/> for a non-null value.
    /// </summary>
    [TestMethod]
    public void ConvertReturnsVisibleForNonNullValue()
    {
        object result = this.converter.Convert("something", typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.AreEqual(Visibility.Visible, result);
    }

    /// <summary>
    /// Verifies that Convert returns <see cref="Visibility.Visible"/> for a null value when inverted.
    /// </summary>
    [TestMethod]
    public void ConvertReturnsVisibleForNullValueWhenInverted()
    {
        object result = this.converter.Convert(null!, typeof(Visibility), "Inverted", CultureInfo.InvariantCulture);

        Assert.AreEqual(Visibility.Visible, result);
    }

    /// <summary>
    /// Verifies that Convert returns <see cref="Visibility.Collapsed"/> for a non-null value when inverted.
    /// </summary>
    [TestMethod]
    public void ConvertReturnsCollapsedForNonNullValueWhenInverted()
    {
        object result = this.converter.Convert("something", typeof(Visibility), "Inverted", CultureInfo.InvariantCulture);

        Assert.AreEqual(Visibility.Collapsed, result);
    }

    /// <summary>
    /// Verifies that ConvertBack throws a <see cref="NotImplementedException"/>.
    /// </summary>
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
