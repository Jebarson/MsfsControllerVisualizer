// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Tests.Xaml;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Msfs.ControllerVisualizer.Models;

/// <summary>
/// Unit tests for verifying XAML resource integrity across controller visual files.
/// </summary>
[TestClass]
public class XamlResourceIntegrityTests
{
    private static readonly string AssetsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Controllers");
    private static readonly Regex StaticResourcePattern = new(@"\{StaticResource\s+(\w+)\}", RegexOptions.Compiled);
    private static readonly Regex XKeyPattern = new(@"x:Key=""(\w+)""", RegexOptions.Compiled);
    private static readonly Regex BasedOnPattern = new(@"BasedOn=""\{StaticResource\s+(\w+)\}""", RegexOptions.Compiled);

    /// <summary>
    /// Verifies that each controller XAML file references only existing static resources.
    /// </summary>
    /// <param name="xamlFile">The controller visual XAML file name.</param>
    [TestMethod]
    [DataRow("HoneycombAlpha.xaml")]
    [DataRow("HoneycombBravo.xaml")]
    [DataRow("SaitekProFlightRudderPedals2024.xaml")]
    [DataRow("MfdCougar2024Planes.xaml")]
    public void AllStaticResourceReferencesResolveToExistingKeys(string xamlFile)
    {
        string controllerStylesPath = Path.Combine(AssetsDir, "ControllerStyles.xaml");
        string controllerXamlPath = Path.Combine(AssetsDir, xamlFile);

        Assert.IsTrue(File.Exists(controllerStylesPath), $"ControllerStyles.xaml should exist at {controllerStylesPath}");
        Assert.IsTrue(File.Exists(controllerXamlPath), $"{xamlFile} should exist at {controllerXamlPath}");

        // Collect all resource keys from ControllerStyles.xaml
        HashSet<string> availableKeys = CollectResourceKeys(controllerStylesPath);

        // Collect local resource keys defined within the controller XAML itself
        HashSet<string> localKeys = CollectResourceKeys(controllerXamlPath);
        availableKeys.UnionWith(localKeys);

        // Find all StaticResource references in the controller XAML
        string controllerContent = File.ReadAllText(controllerXamlPath);
        MatchCollection references = StaticResourcePattern.Matches(controllerContent);

        List<string> unresolvedReferences = [];

        foreach (Match match in references)
        {
            string referencedKey = match.Groups[1].Value;

            // Skip system/framework resources (e.g., SystemColors) and converter keys
            if (referencedKey.StartsWith("SystemColors", StringComparison.Ordinal))
            {
                continue;
            }

            if (!availableKeys.Contains(referencedKey))
            {
                unresolvedReferences.Add(referencedKey);
            }
        }

        // Also check BasedOn references
        MatchCollection basedOnMatches = BasedOnPattern.Matches(controllerContent);
        foreach (Match match in basedOnMatches)
        {
            string baseKey = match.Groups[1].Value;
            if (!availableKeys.Contains(baseKey))
            {
                unresolvedReferences.Add($"BasedOn:{baseKey}");
            }
        }

        Assert.AreEqual(
            0,
            unresolvedReferences.Count,
            $"Unresolved StaticResource references in {xamlFile}: {string.Join(", ", unresolvedReferences)}");
    }

    /// <summary>
    /// Verifies that ControllerStyles.xaml does not contain duplicate resource keys.
    /// </summary>
    [TestMethod]
    public void ControllerStylesHasNoDuplicateResourceKeys()
    {
        string stylesPath = Path.Combine(AssetsDir, "ControllerStyles.xaml");
        string content = File.ReadAllText(stylesPath);

        MatchCollection matches = XKeyPattern.Matches(content);
        List<string> allKeys = matches.Select(m => m.Groups[1].Value).ToList();
        List<string> duplicates = allKeys
            .GroupBy(k => k)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.AreEqual(
            0,
            duplicates.Count,
            $"Duplicate resource keys in ControllerStyles.xaml: {string.Join(", ", duplicates)}");
    }

    /// <summary>
    /// Verifies that each controller XAML file merges ControllerStyles.xaml.
    /// </summary>
    /// <param name="xamlFile">The controller visual XAML file name.</param>
    [TestMethod]
    [DataRow("HoneycombAlpha.xaml")]
    [DataRow("HoneycombBravo.xaml")]
    [DataRow("SaitekProFlightRudderPedals2024.xaml")]
    [DataRow("MfdCougar2024Planes.xaml")]
    public void EachControllerXamlMergesControllerStyles(string xamlFile)
    {
        string path = Path.Combine(AssetsDir, xamlFile);
        string content = File.ReadAllText(path);

        Assert.IsTrue(
            content.Contains("ControllerStyles.xaml", StringComparison.OrdinalIgnoreCase),
            $"{xamlFile} should merge ControllerStyles.xaml via MergedDictionaries");
    }

    /// <summary>
    /// Verifies that every visual file referenced in controllers.json exists on disk.
    /// </summary>
    [TestMethod]
    public void AllVisualFilesReferencedInControllersJsonExist()
    {
        string jsonPath = Path.Combine(AssetsDir, "controllers.json");
        Assert.IsTrue(File.Exists(jsonPath), "controllers.json should exist in Assets/Controllers");

        string json = File.ReadAllText(jsonPath);
        ControllersData? data = JsonSerializer.Deserialize<ControllersData>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.IsNotNull(data);
        Assert.IsTrue(data.Controllers.Count > 0, "controllers.json should contain at least one controller");

        foreach (ControllerDefinition controller in data.Controllers)
        {
            string visualPath = Path.Combine(AssetsDir, controller.VisualFile);
            Assert.IsTrue(
                File.Exists(visualPath),
                $"Visual file '{controller.VisualFile}' referenced by '{controller.Name}' should exist");
        }
    }

    /// <summary>
    /// Verifies that each controller XAML file does not contain duplicate local resource keys.
    /// </summary>
    /// <param name="xamlFile">The controller visual XAML file name.</param>
    [TestMethod]
    [DataRow("HoneycombAlpha.xaml")]
    [DataRow("HoneycombBravo.xaml")]
    [DataRow("SaitekProFlightRudderPedals2024.xaml")]
    [DataRow("MfdCougar2024Planes.xaml")]
    public void EachControllerXamlHasNoDuplicateLocalResourceKeys(string xamlFile)
    {
        string path = Path.Combine(AssetsDir, xamlFile);
        string content = File.ReadAllText(path);

        MatchCollection matches = XKeyPattern.Matches(content);
        List<string> allKeys = matches.Select(m => m.Groups[1].Value).ToList();
        List<string> duplicates = allKeys
            .GroupBy(k => k)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.AreEqual(
            0,
            duplicates.Count,
            $"Duplicate resource keys in {xamlFile}: {string.Join(", ", duplicates)}");
    }

    private static HashSet<string> CollectResourceKeys(string xamlFilePath)
    {
        string content = File.ReadAllText(xamlFilePath);
        MatchCollection matches = XKeyPattern.Matches(content);
        return new HashSet<string>(matches.Select(m => m.Groups[1].Value));
    }
}
