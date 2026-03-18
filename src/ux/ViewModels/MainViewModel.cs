// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.ViewModels;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Msfs.ControllerVisualizer.Common;
using Msfs.ControllerVisualizer.Models;
using Msfs.ControllerVisualizer.Services;

/// <summary>
/// MainViewModel manages the primary UI logic for the MSFS Controller Visualizer application.
/// It handles controller discovery, selection, button mapping, and visual rendering.
/// </summary>
public class MainViewModel : NotifyPropertyBase
{
    private const string RelativePathFormat = "..{0}..{0}..";
    private const string AssetsControllersPath = "Assets{0}Controllers";
    private const string XamlExtension = "*.xaml";

    private readonly ControllerDiscoveryService discoveryService;
    private readonly ControllerButtonMapper buttonMapper;
    private readonly ControllerDefinitionLoader definitionLoader;
    private readonly PrintService printService;

    private string exportFolderPath = string.Empty;
    private string statusMessage = "Ready - Please select a folder containing exported MSFS profile XML files";
    private ExportedControllerInfo? selectedExportedController;
    private List<ControllerDefinition> supportedControllers = new();
    private List<ExportedControllerInfo> exportedControllers = new();
    private ControllerDefinition? matchedControllerDefinition;
    private List<ButtonMapping> currentButtonMappings = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// Sets up the required services and command handlers.
    /// </summary>
    public MainViewModel()
    {
        this.discoveryService = new();
        this.buttonMapper = new();
        this.definitionLoader = new();
        this.printService = new();

        this.BrowseForExportFolderCommand = new Command<object>(parameter => this.BrowseForExportFolder());
        this.ReloadVisualCommand = new Command<object>(parameter => this.ReloadVisual());
        this.PrintCommand = new Command<System.Windows.FrameworkElement>(element => this.Print(element));

        this.LoadSupportedControllers();
    }

    /// <summary>
    /// Gets or sets the path to the folder containing exported MSFS profile XML files.
    /// </summary>
    public string ExportFolderPath
    {
        get => this.exportFolderPath;
        set
        {
            if (this.exportFolderPath != value)
            {
                this.exportFolderPath = value;
                this.OnNotifyPropertyChanged();
                this.DiscoverControllers();
            }
        }
    }

    /// <summary>
    /// Gets or sets the current status message displayed to the user.
    /// </summary>
    public string StatusMessage
    {
        get => this.statusMessage;
        set
        {
            if (this.statusMessage != value)
            {
                this.statusMessage = value;
                this.OnNotifyPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the list of supported controller definitions.
    /// </summary>
    public List<ControllerDefinition> SupportedControllers
    {
        get => this.supportedControllers;
        private set
        {
            this.supportedControllers = value;
            this.OnNotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the list of exported controllers discovered in the selected folder.
    /// </summary>
    public List<ExportedControllerInfo> ExportedControllers
    {
        get => this.exportedControllers;
        private set
        {
            this.exportedControllers = value;
            this.OnNotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the currently selected exported controller.
    /// </summary>
    public ExportedControllerInfo? SelectedExportedController
    {
        get => this.selectedExportedController;
        set
        {
            if (this.selectedExportedController != value)
            {
                this.selectedExportedController = value;
                this.OnNotifyPropertyChanged();
                this.OnControllerSelectionChanged();
            }
        }
    }

    /// <summary>
    /// Gets the controller definition matched to the selected exported controller.
    /// </summary>
    public ControllerDefinition? MatchedControllerDefinition
    {
        get => this.matchedControllerDefinition;
        private set
        {
            this.matchedControllerDefinition = value;
            this.OnNotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the list of button mappings for the currently selected controller.
    /// </summary>
    public List<ButtonMapping> CurrentButtonMappings
    {
        get => this.currentButtonMappings;
        private set
        {
            this.currentButtonMappings = value;
            this.OnNotifyPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the command to browse for an export folder.
    /// </summary>
    public ICommand BrowseForExportFolderCommand { get; }

    /// <summary>
    /// Gets the command to reload the visual representation of the controller.
    /// </summary>
    public ICommand ReloadVisualCommand { get; }

    /// <summary>
    /// Gets the command to print the controller layout.
    /// </summary>
    public ICommand PrintCommand { get; }

    private void LoadSupportedControllers()
    {
        this.SupportedControllers = this.definitionLoader.LoadSupportedControllers();

        if (this.SupportedControllers.Count > 0)
        {
            this.StatusMessage = $"Loaded {this.SupportedControllers.Count} supported controller(s): {string.Join(", ", this.SupportedControllers.Select(c => c.Name))}";
        }
        else
        {
            this.StatusMessage = "Warning: No controller definitions loaded. Check Assets/Controllers/controllers.json";
        }
    }

    private void BrowseForExportFolder()
    {
        System.Windows.Forms.FolderBrowserDialog dialog = new()
        {
            Description = "Select folder containing exported MSFS profile XML files",
            ShowNewFolderButton = false,
        };

        System.Windows.Forms.DialogResult result = dialog.ShowDialog();
        if (result == System.Windows.Forms.DialogResult.OK)
        {
            this.ExportFolderPath = dialog.SelectedPath;
        }

        dialog.Dispose();
    }

    private void DiscoverControllers()
    {
        if (string.IsNullOrEmpty(this.ExportFolderPath))
        {
            this.ExportedControllers = new();
            this.SelectedExportedController = null;
            this.MatchedControllerDefinition = null;
            this.CurrentButtonMappings = new();
            return;
        }

        try
        {
            List<ExportedControllerInfo> supported = this.discoveryService.GetSupportedControllers(
                this.ExportFolderPath,
                this.SupportedControllers);

            this.ExportedControllers = supported;

            if (supported.Count > 0)
            {
                this.SelectedExportedController = supported[0];

                string[] uniqueFiles = supported.Select(c => c.FileName).Distinct().ToArray();
                this.StatusMessage = $"Found {supported.Count} supported controller(s) in {uniqueFiles.Length} file(s)";
            }
            else
            {
                List<ExportedControllerInfo> allControllers = this.discoveryService.DiscoverControllersInFolder(this.ExportFolderPath);

                if (allControllers.Count > 0)
                {
                    string foundDevices = string.Join(", ", allControllers.Select(c => c.DisplayName));
                    string supportedDevices = string.Join(", ", this.SupportedControllers.Select(c => c.Name));
                    this.StatusMessage = $"Found {allControllers.Count} controller(s) [{foundDevices}] but none are supported. Supported: {supportedDevices}";
                }
                else
                {
                    string[] xmlFiles = Directory.GetFiles(this.ExportFolderPath, "*.xml", SearchOption.TopDirectoryOnly);
                    if (xmlFiles.Length > 0)
                    {
                        this.StatusMessage = $"Found {xmlFiles.Length} XML file(s) but no valid controller profiles. Please check the files.";
                    }
                    else
                    {
                        this.StatusMessage = "No XML files found in the selected folder. Please export your profiles from MSFS 2024 first.";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            this.StatusMessage = $"Error scanning folder: {ex.Message}";
            this.ExportedControllers = new();
            this.SelectedExportedController = null;
        }
    }

    private void OnControllerSelectionChanged()
    {
        // Clear previous state
        this.MatchedControllerDefinition = null;
        this.CurrentButtonMappings = new();

        if (this.SelectedExportedController == null)
        {
            this.StatusMessage = "No controller selected";
            return;
        }

        // Match controller to definition
        ControllerDefinition? matched = this.discoveryService.MatchController(
            this.SelectedExportedController,
            this.SupportedControllers);

        if (matched == null)
        {
            this.StatusMessage = $"Selected: {this.SelectedExportedController.DisplayName} → No matching controller definition";
            return;
        }

        this.MatchedControllerDefinition = matched;

        // Load mappings automatically
        this.LoadMappingsForSelectedController();
    }

    private void LoadMappingsForSelectedController()
    {
        if (this.SelectedExportedController?.DeviceElement == null || this.MatchedControllerDefinition == null)
        {
            this.CurrentButtonMappings = new();
            this.StatusMessage = "Cannot load mappings - missing controller data";
            return;
        }

        try
        {
            List<ButtonMapping> mappings = this.buttonMapper.MapButtons(
                this.SelectedExportedController.DeviceElement,
                this.MatchedControllerDefinition);

            this.CurrentButtonMappings = mappings;

            int mappedActions = mappings.Count;

            this.StatusMessage = $"{this.MatchedControllerDefinition.Name}: {mappedActions} actions mapped ({this.SelectedExportedController.FileName})";
        }
        catch (Exception ex)
        {
            this.StatusMessage = $"Error mapping buttons: {ex.Message}";
            this.CurrentButtonMappings = new();
        }
    }

    private void ReloadVisual()
    {
        try
        {
            // First, copy XAML files from source to output directory
            this.CopyControllerXamlFiles();

            // Save current values
            ControllerDefinition? savedDefinition = this.MatchedControllerDefinition;
            List<ButtonMapping> savedMappings = this.CurrentButtonMappings;

            // Clear to trigger binding update
            this.MatchedControllerDefinition = null;
            this.CurrentButtonMappings = new();

            // Force UI update
            System.Windows.Application.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);

            // Restore values - this will cause the converter to re-run and reload XAML from disk
            this.MatchedControllerDefinition = savedDefinition;
            this.CurrentButtonMappings = savedMappings;

            this.StatusMessage = $"Visual reloaded from source at {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            this.StatusMessage = $"Error reloading visual: {ex.Message}";
        }
    }

    private void CopyControllerXamlFiles()
    {
        // Get the source directory (project directory)
        // AppDomain.CurrentDomain.BaseDirectory is typically: X:\...\bin\Debug\net10.0-windows\
        // We need to go up to the project root
        string binDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string projectDirectory = Path.GetFullPath(Path.Combine(binDirectory, RelativePathFormat));

        string sourceControllersPath = Path.Combine(projectDirectory, string.Format(AssetsControllersPath, Path.DirectorySeparatorChar));
        string targetControllersPath = Path.Combine(binDirectory, string.Format(AssetsControllersPath, Path.DirectorySeparatorChar));

        if (!Directory.Exists(sourceControllersPath))
        {
            throw new DirectoryNotFoundException($"Source directory not found: {sourceControllersPath}");
        }

        // Ensure target directory exists
        Directory.CreateDirectory(targetControllersPath);

        // Copy all XAML files
        string[] xamlFiles = Directory.GetFiles(sourceControllersPath, XamlExtension, SearchOption.TopDirectoryOnly);

        foreach (string sourceFile in xamlFiles)
        {
            string fileName = Path.GetFileName(sourceFile);
            string targetFile = Path.Combine(targetControllersPath, fileName);

            // Copy and overwrite
            File.Copy(sourceFile, targetFile, overwrite: true);
            System.Diagnostics.Debug.WriteLine($"Copied: {fileName}");
        }
    }

    private void Print(System.Windows.FrameworkElement visualElement)
    {
        if (visualElement == null)
        {
            this.StatusMessage = "Error: Cannot print - no controller visual loaded";
            return;
        }

        try
        {
            string controllerName = this.MatchedControllerDefinition?.Name ?? "Controller";
            string documentTitle = $"{controllerName} Layout";

            bool success = this.printService.Print(visualElement, documentTitle);

            if (success)
            {
                this.StatusMessage = $"Printed {controllerName} layout at {DateTime.Now:HH:mm:ss}";
            }
        }
        catch (Exception ex)
        {
            this.StatusMessage = $"Print error: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Print error: {ex.Message}");
        }
    }
}
