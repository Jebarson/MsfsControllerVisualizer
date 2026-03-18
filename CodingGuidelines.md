# Coding Guidelines

This document defines the coding standards and conventions for the **Msfs.ControllerVisualizer** project.
All contributors must adhere to these guidelines when writing or modifying code.

---

## 1. Microsoft C# Naming Conventions

Follow the official Microsoft C# naming conventions as published at:

- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET Naming Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines)
- [C# Identifier Naming Rules](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)

**Key points from the above references:**

- **PascalCase** for public members, types, namespaces, methods, properties, events, and enum values.
- **camelCase** for private/internal fields, local variables, and method parameters.
- **No underscores** in identifiers (fields, variables, methods, etc.).
- Interface names must begin with an uppercase `I` (e.g., `ICommand`).
- Async method names must end with `Async` (e.g., `LoadDataAsync`).
- Boolean properties and methods should read as assertions (e.g., `IsVisible`, `HasValue`, `CanExecute`).
- Constants should use **PascalCase** (e.g., `MaxRetryCount`).
- Type parameters should use `T` or descriptive names prefixed with `T` (e.g., `TResult`).
- File-scoped namespaces are preferred (e.g., `namespace Foo.Bar;`).
- `using` directives should be placed inside the namespace declaration (file-scoped style) to scope imports.

---

## 2. Instance Member Qualification with `this.`

All instance member accesses (fields, properties, methods, events) **must** be preceded with `this.` when referenced within the same class.

```csharp
// ✅ Correct
this.InitializeComponent();
this.DataContext = new MainViewModel();
this.OnNotifyPropertyChanged();

// ❌ Incorrect
InitializeComponent();
DataContext = new MainViewModel();
OnNotifyPropertyChanged();
```

This applies to all code including production code and test code (e.g., `this.converter`, `this.service`).

---

## 3. Explicit Types — No `var`

**Absolutely no usage of `var`** unless creating a simple anonymous/dynamic object.

All variable declarations must use concrete, explicit types. The only permitted exception is anonymous types:

```csharp
// ✅ Allowed — anonymous type
var anon = new { Name = "Test", Value = 42 };

// ✅ Correct — explicit types
List<ButtonMapping> mappings = new();
ControllerDefinition? found = loader.GetControllerById(id, controllers);
string filePath = Path.Combine(folder, "file.xml");

// ❌ Incorrect — var with known types
var mappings = new List<ButtonMapping>();
var found = loader.GetControllerById(id, controllers);
var filePath = Path.Combine(folder, "file.xml");
```

---

## 4. Single Type Per File

A single file must contain **no more than one type** definition (class, struct, enum, interface, record, delegate, etc.).

- The file name must match the type name (e.g., `ButtonMapping.cs` contains `class ButtonMapping`).
- Nested types defined within a parent type are permitted within the same file.

---

## 5. Purposeful Unit Tests Only

**Do not write unit tests for models that serve no functional purpose.** Pure data classes with only auto-properties and no logic (no computed properties, no methods, no validation) do not require dedicated test files.

Models with computed properties, custom logic, or non-trivial behavior **should** be tested.

```csharp
// ❌ No tests needed — pure data class
public class ButtonMapping
{
    public string ButtonId { get; set; } = string.Empty;
    public string MsfsCommand { get; set; } = string.Empty;
}

// ✅ Tests warranted — has computed property logic
public class ExportedControllerInfo
{
    public string DisplayName => /* branching logic */;
}
```

---

## 6. Simplified `new` Expressions

Use target-typed `new()` expressions (simplified new) wherever the type can be inferred from the declaration context.

```csharp
// ✅ Correct — simplified new
List<ButtonMapping> mappings = new();
ControllerButtonMapper mapper = new();
JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
ExportedControllerInfo info = new() { DeviceName = "Test" };

// ❌ Incorrect — redundant type specification
List<ButtonMapping> mappings = new List<ButtonMapping>();
ControllerButtonMapper mapper = new ControllerButtonMapper();
JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
```

---

## 7. Copyright Information

**All files** must include the following copyright header as the very first lines:

```csharp
// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.
```

This applies to all `.cs` files in both the main project and test project.

---

## 8. XML Documentation

All **non-private** types, properties, and methods must have clear XML documentation comments using `<summary>`, `<param>`, `<returns>`, and other appropriate XML doc tags.

```csharp
/// <summary>
/// Provides services for mapping controller buttons to MSFS commands.
/// </summary>
public class ControllerButtonMapper
{
    /// <summary>
    /// Maps all button bindings from a device XML element.
    /// </summary>
    /// <param name="deviceElement">The XML element containing the device configuration.</param>
    /// <param name="controllerDefinition">The controller definition.</param>
    /// <returns>A list of button mappings extracted from the configuration.</returns>
    public List<ButtonMapping> MapButtons(XElement deviceElement, ControllerDefinition controllerDefinition)
    {
        // ...
    }
}
```

**Test classes** must have XML documentation on the class declaration. Test methods are exempt as their descriptive names serve as documentation by MSTest naming convention.

---

## Summary

| # | Guideline | Scope |
|---|-----------|-------|
| 1 | Microsoft C# naming conventions | All code |
| 2 | `this.` qualification for instance members | All code |
| 3 | No `var` (except anonymous types) | All code |
| 4 | One type per file | All code |
| 5 | No tests for logic-free models | Test code |
| 6 | Simplified `new()` expressions | All code |
| 7 | Copyright header in every file | All files |
| 8 | XML documentation on non-private members | All code (test methods exempt) |
