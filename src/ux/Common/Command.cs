// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Common;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

/// <summary>
/// Class for binding all the commands to do filtered command binding. Implements the <see cref="System.Windows.Input.ICommand"/> Implements the
/// <see cref="System.IDisposable"/>
/// </summary>
/// <typeparam name="T">The type of command parameter.</typeparam>
/// <seealso cref="System.Windows.Input.ICommand"/>
/// <seealso cref="System.IDisposable"/>
internal class Command<T> : ICommand, IDisposable
{
    private readonly Predicate<T>? canExecute;
    private readonly Action<T> execute;
    private bool isDisposed;
    private INotifyPropertyChanged? objectToListen;

    /// <summary>
    /// Initializes a new instance of the <see cref="Command{T}"/> class.
    /// </summary>
    /// <param name="execute">The execute method.</param>
    public Command(Action<T> execute)
      : this(execute, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Command{T}"/> class.
    /// </summary>
    /// <param name="execute">The execute method.</param>
    /// <param name="canExecute">The can execute method.</param>
    public Command(Action<T> execute, Predicate<T>? canExecute) : this(execute, canExecute, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Command{T}"/> class.
    /// </summary>
    /// <param name="execute">The execute method.</param>
    /// <param name="canExecute">The can execute method.</param>
    /// <param name="objectToListen">The object to listen.</param>
    /// <param name="propertiesToListen">
    /// The properties to listen. Null if all the properties are to be listened.
    /// </param>
    public Command(Action<T> execute, Predicate<T>? canExecute, INotifyPropertyChanged? objectToListen, IEnumerable<string>? propertiesToListen = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
        this.objectToListen = objectToListen;

        if (this.objectToListen != null)
        {
            this.objectToListen.PropertyChanged += this.ObjectPropertyChanged;
            this.PropertiesToListen = propertiesToListen;
        }
    }

    /// <summary>
    /// Occurs when changes occur that affect whether or not the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add
        {
            this.CanExecuteChangeValue += value;
        }

        remove
        {
            this.CanExecuteChangeValue -= value;
        }
    }

    /// <summary>
    /// Occurs when [can execute change value].
    /// </summary>
    private event EventHandler? CanExecuteChangeValue;

    /// <summary>
    /// Gets the properties to listen.
    /// </summary>
    /// <value>The properties to listen.</value>
    public IEnumerable<string>? PropertiesToListen { get; private set; }

    /// <summary>
    /// Defines the method that determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, this object
    /// can be set to <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if this command can be executed; otherwise, <see langword="false"/>.
    /// </returns>
    public bool CanExecute(object? parameter) => this.canExecute?.Invoke((T)parameter!) ?? true;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting
    /// unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, this object
    /// can be set to <see langword="null"/>.
    /// </param>
    public void Execute(object? parameter) => this.execute((T)parameter!);

    /// <summary>
    /// Raises the can execute changed.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        this.CanExecuteChangeValue.RaiseEvent(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release
    /// only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (this.isDisposed)
        {
            return;
        }

        if (disposing)
        {
            if (this.objectToListen != null)
            {
                this.objectToListen.PropertyChanged -= this.ObjectPropertyChanged;
            }

            this.objectToListen = null;
        }

        this.isDisposed = true;
    }

    /// <summary>
    /// Listens to any property changed of the object that is passed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">
    /// The <see cref="PropertyChangedEventArgs"/> instance containing the event data.
    /// </param>
    private void ObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (this.PropertiesToListen == null || ((this.PropertiesToListen?.Any() ?? false) && this.PropertiesToListen.Contains(e.PropertyName)))
        {
            // Raise on all the property change
            this.RaiseCanExecuteChanged();
        }
    }
}
