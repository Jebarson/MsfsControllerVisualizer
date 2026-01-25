// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Common;

/// <summary>
/// Static utility class for event handling extensions.
/// Provides methods to safely invoke event handlers with null-coalescing syntax.
/// </summary>
public static class EventExtensions
{
    /// <summary>
    /// Safely raises an EventHandler with the specified sender.
    /// </summary>
    /// <param name="handler">The event handler to raise.</param>
    /// <param name="sender">The object raising the event.</param>
    public static void RaiseEvent(this EventHandler? handler, object sender)
    {
        handler?.Invoke(sender, EventArgs.Empty);
    }

    /// <summary>
    /// Safely raises a generic EventHandler with the specified sender and event arguments.
    /// </summary>
    /// <typeparam name="T">The type of EventArgs.</typeparam>
    /// <param name="handler">The event handler to raise.</param>
    /// <param name="sender">The object raising the event.</param>
    /// <param name="args">The event arguments.</param>
    public static void RaiseEvent<T>(this EventHandler<T>? handler, object sender, T args) where T : EventArgs
    {
        handler?.Invoke(sender, args);
    }
}
