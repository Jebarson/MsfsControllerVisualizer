// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer;

using System.Text;
using System.Windows;
using Msfs.ControllerVisualizer.ViewModels;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// Serves as the main application window and hosts the primary UI.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// Sets up the main view model and UI layout.
    /// </summary>
    public MainWindow()
    {
        this.InitializeComponent();
        this.DataContext = new MainViewModel();
    }

    /// <summary>
    /// Gets a value indicating whether the application is running in debug mode.
    /// </summary>
    public bool IsDebugMode
    {
        get
        {
#if DEBUG
            return true;
#else
                return false;
#endif
        }
    }
}