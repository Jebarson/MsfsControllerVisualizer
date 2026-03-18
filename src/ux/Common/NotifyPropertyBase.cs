// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Common;

using System.ComponentModel;
using System.Runtime.CompilerServices;

/// <summary>
/// Base to implement property change notification. Implements the <see cref="System.ComponentModel.INotifyPropertyChanged"/>
/// </summary>
/// <seealso cref="System.ComponentModel.INotifyPropertyChanged"/>
public class NotifyPropertyBase : INotifyPropertyChanged
{
    /// <summary>
    /// Occurs when [property changed].
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Called when [notify property changed].
    /// </summary>
    /// <param name="name">The name.</param>
    internal void OnNotifyPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

