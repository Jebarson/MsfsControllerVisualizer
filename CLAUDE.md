# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MSFS Controller Visualizer is a WPF application that visualizes Microsoft Flight Simulator 2024 controller bindings. It parses exported MSFS profile XML files and displays button mappings on graphical representations of supported flight controllers.

**Technology Stack:**
- .NET 10.0 (net10.0-windows)
- WPF (Windows Presentation Foundation)
- Windows Forms (for folder browser dialog)
- Target: Windows desktop only

## Repository Structure

```

.github/
  workflows/
    Release.yml                   # GitHub Actions release workflow
src/
  Msfs.ControllerVisualizer.slnx    # Solution file
  ux/                               # WPF application project
    Msfs.ControllerVisualizer.csproj
    App.xaml / App.xaml.cs
    MainWindow.xaml / MainWindow.xaml.cs
    AssemblyInfo.cs
    Assets/Controllers/             # Controller definitions and visuals
      controllers.json              # Controller metadata definitions
      ControllerStyles.xaml         # Shared styles for controller visuals
      HoneycombAlpha.xaml
      HoneycombBravo.xaml
      SaitekProFlightRudderPedals2024.xaml
      MfdCougar2024Planes.xaml
      Images/                       # Reference images (not used in app)
    Common/                         # Shared utilities
      Command.cs, EventExtensions.cs, NotifyPropertyBase.cs
    Converters/                     # WPF value converters
      ControllerVisualConverter.cs
      JoystickButtonMappingConverter.cs
      JoystickButtonTruncateConverter.cs
      NullToVisibilityConverter.cs
    Models/                         # Data models
      ButtonMapping.cs, ControllerDefinition.cs
      ControllersData.cs, ExportedControllerInfo.cs
    Services/                       # Business logic
      ControllerButtonMapper.cs
      ControllerDefinitionLoader.cs
      ControllerDiscoveryService.cs
      PrintService.cs
    ViewModels/
      MainViewModel.cs
  test/                             # Unit test project (MSTest)
    Msfs.ControllerVisualizer.Tests.csproj
    Converters/                     # Converter tests
    Models/                         # Model tests
    Services/                       # Service tests
    Xaml/                           # XAML loading and integrity tests
    Mocks/                          # Sample MSFS XML files for testing
CLAUDE.md                             # This file (Claude Code guidance)
CodingGuidelines.md                   # Coding standards and conventions
COPYRIGHT.md                          # Copyright and license terms
.editorconfig
.gitignore
```


## Related Documentation

- **[CodingGuidelines.md](CodingGuidelines.md)** - Coding standards, naming conventions, formatting rules, and style requirements for all code in this project.
- **[COPYRIGHT.md](COPYRIGHT.md)** - Copyright and license terms. All source files reference this via their copyright header.

## Build and Run Commands

All commands should be run from the `src/` directory:

```powershell
# Build the solution
cd src
dotnet build

# Run the application
dotnet run --project ux

# Run tests
dotnet test

# Clean build artifacts
dotnet clean

# Publish for deployment
dotnet publish -c Release --project ux
```


## Architecture

### Data Flow

1. **Discovery Phase** (ControllerDiscoveryService)
   - Scans user-selected folder for MSFS exported XML files
   - Parses XML to extract Device elements with DeviceName/ProductID attributes
   - Matches discovered devices against supported controller definitions from `Assets/Controllers/controllers.json`
   - Merges multiple profiles for the same device into a single consolidated representation

2. **Mapping Phase** (ControllerButtonMapper)
   - Extracts button bindings from XML: `Device/Context/Action/Primary/KEY` elements
   - Converts KEY Information attribute (e.g., `Joystick Button 35`) to normalized identifiers (e.g., `Joystick_Button_35`)
   - Creates ButtonMapping records linking button IDs to MSFS commands

3. **Rendering Phase** (ControllerVisualConverter)
   - Dynamically loads controller XAML visuals from `Assets/Controllers/`
   - Injects button mappings via static converter state before XAML parsing
   - XAML bindings use JoystickButtonMappingConverter to display command names on visual elements
   - JoystickButtonTruncateConverter handles text truncation for display

### Key Components

**Services:**
- `ControllerDiscoveryService`: XML parsing, device matching, profile merging
- `ControllerDefinitionLoader`: JSON deserialization of controller definitions
- `ControllerButtonMapper`: Button binding extraction and normalization
- `PrintService`: Printing controller layouts via PrintDialogX

**Models:**
- `ControllerDefinition`: Metadata for supported controllers (name, ProductID, XAML visual file)
- `ExportedControllerInfo`: Discovered device info with source XML element
- `ButtonMapping`: Links button ID to MSFS command and friendly name
- `ControllersData`: Root structure for controllers.json deserialization

**Converters:**
- `ControllerVisualConverter`: Loads XAML visuals and sets up mapping context (IMultiValueConverter)
- `JoystickButtonMappingConverter`: Looks up command names for button IDs using static mapping state
- `JoystickButtonTruncateConverter`: Text truncation with mapping awareness
- `NullToVisibilityConverter`: Standard WPF visibility converter

**ViewModels:**
- `MainViewModel`: Orchestrates discovery, selection, and mapping; exposes properties for UI binding

### XML Processing

The XML parser handles MSFS export files with multiple root elements by wrapping content in a temporary `<Root>` element. Key XML structure:

```xml
<Device DeviceName="..." ProductID="..." GUID="...">
  <Context ContextName="...">
    <Action ActionName="KEY_...">
      <Primary>
        <KEY Information="Joystick Button N" />
      </Primary>
    </Action>
  </Context>
</Device>
```


Button identifiers are normalized by replacing spaces with underscores: `Joystick Button 35` becomes `Joystick_Button_35`

### XAML Visual System

Controller visuals are UserControl XAML files in `Assets/Controllers/`. Button elements use ConverterParameter to specify button identifiers:

```xaml
<TextBlock Text="{Binding Converter={StaticResource JoystickButtonMappingConverter},
                          ConverterParameter=Joystick_Button_35}" />
```


The converters rely on static state set by ControllerVisualConverter before XAML parsing. This is necessary because XAML binding contexts cannot be easily propagated to converter parameters during dynamic loading.

## Adding New Controllers

1. Add controller metadata to `src/ux/Assets/Controllers/controllers.json`:
   - `name`: Display name
   - `deviceName`: Substring to match against XML DeviceName attribute
   - `productId`: USB Product ID for precise matching
   - `visualFile`: XAML filename

2. Create XAML visual file in `src/ux/Assets/Controllers/`:
   - Define UserControl with button visuals
   - Use `{StaticResource JoystickButtonMappingConverter}` with `ConverterParameter=Joystick_Button_N`
   - The XAML is the single source of truth for button definitions and labels

3. Visual file must include converter namespace:
   ```xaml
   xmlns:converters="clr-namespace:Msfs.ControllerVisualizer.Converters;assembly=Msfs.ControllerVisualizer"
   ```


## Important Implementation Details

- **Static Converter State**: JoystickButtonMappingConverter and JoystickButtonTruncateConverter use static fields to hold current mappings. This is set by ControllerVisualConverter before XAML parsing. This approach is necessary for dynamic XAML loading but means the converters are not thread-safe or multi-instance safe.

- **Profile Merging**: When multiple XML files contain the same device (matched by DeviceName), the service merges all Context elements into a single consolidated DeviceElement. This allows aggregating bindings from multiple MSFS profiles.

- **Matching Priority**: Controllers are matched first by ProductID (most specific), then by DeviceName substring match (less specific).

- **XML Declaration Handling**: The XML parser strips `<?xml ...?>` declarations before wrapping content to avoid parsing errors with multiple root elements.

- **Debug Reload Button**: A 20x20 reload button appears in the bottom-right corner of the controller visual area when running in DEBUG mode. Clicking it copies all XAML files from the source project directory to the output directory, then forces the ControllerVisualConverter to re-run by temporarily clearing bound properties. This allows editing XAML files and seeing changes immediately without rebuilding. The button visibility is controlled by `IsDebugMode` in MainWindow.xaml.cs using conditional compilation. The copy mechanism is in `CopyControllerXamlFiles()` which navigates from `bin/Debug/net10.0-windows/` up to the project root.
