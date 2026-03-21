# Design Document

## Purpose

This document describes the solution structure, architecture, and key implementation details for `Msfs.ControllerVisualizer`.

## Project Overview

`Msfs.ControllerVisualizer` is a Windows desktop application for visualizing Microsoft Flight Simulator 2024 controller bindings. It reads exported MSFS controller profile XML files, identifies supported devices, extracts button mappings, and renders those mappings on controller-specific XAML visuals.

## Technology Stack

- `.NET 10`
- `WPF`
- `MSTest`
- Windows desktop only

## Repository Structure

```text
X:\Msfs.ControllerVisualizer\
├── .github/
│   └── workflows/
├── docs/
│   ├── README.md
│   ├── DesignDocument.md
│   ├── CodingGuidelines.md
│   └── Copyright.md
└── src/
    ├── Msfs.ControllerVisualizer.slnx
    ├── ux/
    │   ├── Assets/
    │   │   └── Controllers/
    │   ├── Common/
    │   ├── Converters/
    │   ├── Models/
    │   ├── Services/
    │   └── ViewModels/
    └── test/
        ├── Converters/
        ├── Models/
        ├── Services/
        ├── Xaml/
        └── Mocks/
```

## Solution Organization

### `src/ux`

This is the main WPF application.

- `Assets/Controllers/`
  - Controller metadata in `controllers.json`
  - Controller-specific XAML visuals
  - Shared controller styles
- `Common/`
  - Shared utility types such as command and notification helpers
- `Converters/`
  - WPF converters used during controller visual rendering
- `Models/`
  - Data structures for discovered devices, supported definitions, and button mappings
- `Services/`
  - XML discovery, controller definition loading, button mapping, and printing logic
- `ViewModels/`
  - Application orchestration and UI state

### `src/test`

This is the MSTest project.

- `Services/` contains service tests
- `Converters/` contains converter tests
- `Models/` contains model tests
- `Xaml/` contains XAML integrity tests
- `Mocks/` contains sample MSFS XML exports used by tests

## Runtime Flow

### 1. Discovery

`ControllerDiscoveryService` scans exported XML files, parses `Device` elements, extracts values such as `DeviceName` and `ProductID`, and matches discovered devices against supported controller definitions.

If multiple XML exports represent the same device, matching contexts are merged into one consolidated device representation.

### 2. Mapping

`ControllerButtonMapper` reads button bindings from XML elements under paths such as `Device/Context/Action/Primary/KEY`.

Button names are normalized before use. For example:

- `Joystick Button 35` → `Joystick_Button_35`

The result is a collection of `ButtonMapping` instances used by the UI.

### 3. Rendering

`ControllerVisualConverter` dynamically loads the controller XAML visual for the selected device.

Before parsing the XAML, it pushes the active button mappings into converter state so bindings inside the XAML can resolve controller button IDs to display text.

## Core Components

### Services

- `ControllerDiscoveryService`
  - Finds controller XML files
  - Parses devices
  - Matches supported controllers
  - Merges multi-profile exports
- `ControllerDefinitionLoader`
  - Loads controller definitions from `controllers.json`
- `ControllerButtonMapper`
  - Extracts and normalizes button mappings
- `PrintService`
  - Handles printing of rendered controller layouts

### Models

- `ControllerDefinition`
  - Supported controller metadata
- `ExportedControllerInfo`
  - Parsed controller information from exported XML
- `ButtonMapping`
  - Normalized button-to-command mapping
- `ControllersData`
  - Root object for controller definition deserialization

### Converters

- `ControllerVisualConverter`
  - Loads controller visuals dynamically
- `JoystickButtonMappingConverter`
  - Resolves button IDs to command text
- `JoystickButtonTruncateConverter`
  - Applies display-friendly truncation
- `NullToVisibilityConverter`
  - Standard visibility conversion

### ViewModel

- `MainViewModel`
  - Coordinates folder selection, controller selection, discovery, mapping, and rendering

## Important Design Details

### XML Handling

MSFS export files can contain multiple root-level elements. The parser handles this by wrapping content in a temporary root element before parsing.

### Controller Matching Priority

Controller matching follows this order:

1. `ProductID`
2. `DeviceName`

This keeps exact matches preferred over broader name-based matches.

### Profile Merging

If multiple XML files belong to the same physical controller, their `Context` elements are merged into one device model so all bindings can be displayed together.

### Dynamic XAML Rendering

Controller visuals are stored as XAML files in `src/ux/Assets/Controllers/`. Each visual uses converter parameters to identify controller buttons.

Example pattern:

```xaml
<TextBlock Text="{Binding Converter={StaticResource JoystickButtonMappingConverter}, ConverterParameter=Joystick_Button_35}" />
```

### Static Converter State

The controller visual rendering approach uses shared converter state set immediately before XAML loading. This simplifies dynamic rendering, but it is not designed for multi-instance or multi-threaded rendering.

### Debug Reload Support

In debug builds, the application exposes a reload mechanism to copy controller XAML files from the source project into the output location and refresh the rendered visual without a full rebuild.

## Adding a New Controller

1. Add a new controller entry to `src/ux/Assets/Controllers/controllers.json`
2. Add the corresponding XAML visual under `src/ux/Assets/Controllers/`
3. Add representative XML test data under `src/test/Mocks/`
4. Add or update tests under `src/test/Services/` and any related XAML validation tests

## Build and Test

Run from `src/`:

```powershell
dotnet build

dotnet test

dotnet run --project ux
```

## Related Documents

- `README.md` for doc entry points
- `CodingGuidelines.md` for code conventions
- `Copyright.md` for licensing
