namespace Msfs.ControllerVisualizer.ViewModels;

using System.IO;
using System.Windows.Input;
using Msfs.ControllerVisualizer.Models;
using Msfs.ControllerVisualizer.Services;
using Msfs.ControllerVisualizer.Common;

public class MainViewModel : NotifyPropertyBase
{
    private readonly ControllerDiscoveryService discoveryService;
    private readonly ControllerButtonMapper buttonMapper;
    private readonly ControllerDefinitionLoader definitionLoader;

    private string exportFolderPath = string.Empty;
    private string statusMessage = "Ready - Please select a folder containing exported MSFS profile XML files";
    private ExportedControllerInfo? selectedExportedController;
    private List<ControllerDefinition> supportedControllers = new();
    private List<ExportedControllerInfo> exportedControllers = new();
    private ControllerDefinition? matchedControllerDefinition;
    private List<ButtonMapping> currentButtonMappings = new();

    public MainViewModel()
    {
        this.discoveryService = new ControllerDiscoveryService();
        this.buttonMapper = new ControllerButtonMapper();
        this.definitionLoader = new ControllerDefinitionLoader();

        this.BrowseForExportFolderCommand = new Command<object>(parameter => this.BrowseForExportFolder());

        this.LoadSupportedControllers();
    }

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

    public List<ControllerDefinition> SupportedControllers
    {
        get => this.supportedControllers;
        private set
        {
            this.supportedControllers = value;
            this.OnNotifyPropertyChanged();
        }
    }

    public List<ExportedControllerInfo> ExportedControllers
    {
        get => this.exportedControllers;
        private set
        {
            this.exportedControllers = value;
            this.OnNotifyPropertyChanged();
        }
    }

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

    public ControllerDefinition? MatchedControllerDefinition
    {
        get => this.matchedControllerDefinition;
        private set
        {
            this.matchedControllerDefinition = value;
            this.OnNotifyPropertyChanged();
        }
    }

    public List<ButtonMapping> CurrentButtonMappings
    {
        get => this.currentButtonMappings;
        private set
        {
            this.currentButtonMappings = value;
            this.OnNotifyPropertyChanged();
        }
    }

    public ICommand BrowseForExportFolderCommand { get; }

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
        using System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select folder containing exported MSFS profile XML files",
            ShowNewFolderButton = false
        };

        System.Windows.Forms.DialogResult result = dialog.ShowDialog();
        if (result == System.Windows.Forms.DialogResult.OK)
        {
            this.ExportFolderPath = dialog.SelectedPath;
        }
    }

    private void DiscoverControllers()
    {
        if (string.IsNullOrEmpty(this.ExportFolderPath))
        {
            this.ExportedControllers = new List<ExportedControllerInfo>();
            this.SelectedExportedController = null;
            this.MatchedControllerDefinition = null;
            this.CurrentButtonMappings = new List<ButtonMapping>();
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
            this.ExportedControllers = new List<ExportedControllerInfo>();
            this.SelectedExportedController = null;
        }
    }

    private void OnControllerSelectionChanged()
    {
        // Clear previous state
        this.MatchedControllerDefinition = null;
        this.CurrentButtonMappings = new List<ButtonMapping>();

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
            this.CurrentButtonMappings = new List<ButtonMapping>();
            this.StatusMessage = "Cannot load mappings - missing controller data";
            return;
        }

        try
        {
            List<ButtonMapping> mappings = this.buttonMapper.MapButtons(
                this.SelectedExportedController.DeviceElement, 
                this.MatchedControllerDefinition);

            this.CurrentButtonMappings = mappings;

            int totalButtons = this.MatchedControllerDefinition.Buttons.Count;
            int mappedButtons = mappings.Count;
            
            this.StatusMessage = $"{this.MatchedControllerDefinition.Name}: {mappedButtons} of {totalButtons} buttons mapped ({this.SelectedExportedController.FileName})";
        }
        catch (Exception ex)
        {
            this.StatusMessage = $"Error mapping buttons: {ex.Message}";
            this.CurrentButtonMappings = new List<ButtonMapping>();
        }
    }
}
