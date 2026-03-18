// Copyright (c) 2024 Jebarson. All rights reserved.
// Licensed under terms specified in COPYRIGHT.md - Free for personal use only.

namespace Msfs.ControllerVisualizer.Services;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PrintDialogX;
using PrintDialogX.Enums;
using PrintDialog = PrintDialogX.PrintDialog;

/// <summary>
/// Provides services for printing visual elements using PrintDialogX.
/// Offers lightning-fast real-time preview with modern print dialog features.
/// </summary>
public class PrintService
{
    private const string DefaultDocumentTitle = "Controller Layout";

    /// <summary>
    /// Opens PrintDialogX and prints the specified visual element.
    /// </summary>
    /// <param name="visualElement">The visual element to print (e.g., Viewbox, Canvas, UserControl).</param>
    /// <param name="documentTitle">The title of the print job.</param>
    /// <returns>True if printing was successful, false if cancelled or failed.</returns>
    public bool Print(FrameworkElement visualElement, string documentTitle = DefaultDocumentTitle)
    {
        if (visualElement == null)
        {
            System.Diagnostics.Debug.WriteLine("Print error: visualElement is null");
            return false;
        }

        try
        {
            // Extract child if it's a Viewbox
            FrameworkElement elementToPrint = visualElement;
            if (visualElement is System.Windows.Controls.Viewbox viewbox && viewbox.Child is FrameworkElement child)
            {
                elementToPrint = child;
            }

            System.Diagnostics.Debug.WriteLine($"Creating print document for: {documentTitle}");

            // Create PrintDialogX document
            PrintDocument printDocument = new();
            printDocument.DocumentName = documentTitle;

            // Set document size (standard letter size - 8.5" x 11")
            printDocument.DocumentSize = new PrintDialogX.Enums.Size(PrintDialogX.Enums.Size.DefinedSize.NorthAmericaLetterRotated);

            // Set margins (0.25 inches for minimal whitespace)
            printDocument.DocumentMargin = 24.0; // 0.25 inches at 96 DPI

            // Create a single page with the visual content
            PrintPage page = new();
            page.Content = this.CreatePageContent(elementToPrint);

            printDocument.Pages.Add(page);

            System.Diagnostics.Debug.WriteLine($"Print document created with 1 page");

            // Create and show PrintDialogX with default settings.
            PrintDialog printDialog = new();
            printDialog.PrintSettings.Layout = Layout.Landscape;
            printDialog.PrintSettings.Scale = Scale.AutoFit;
            printDialog.PrintSettings.Quality = Quality.High;
            printDialog.Document = printDocument;
            printDialog.InterfaceSettings.Title = documentTitle;

            System.Diagnostics.Debug.WriteLine($"Opening PrintDialogX for: {documentTitle}");

            // Show the dialog
            bool? result = printDialog.ShowDialog();

            if (result == true && printDialog.Result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"Document printed successfully. Pages printed: {printDialog.Result.PaperCount}");
                return true;
            }

            System.Diagnostics.Debug.WriteLine("Print cancelled or failed");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Print error: {ex.Message}\n{ex.StackTrace}");
            MessageBox.Show($"Failed to print: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    /// <summary>
    /// Creates the page content for printing with proper scaling.
    /// </summary>
    /// <param name="element">The element to print.</param>
    /// <returns>A FrameworkElement ready for printing.</returns>
    private FrameworkElement CreatePageContent(FrameworkElement element)
    {
        // Get element dimensions
        double elementWidth = element.ActualWidth > 0 ? element.ActualWidth : 800;
        double elementHeight = element.ActualHeight > 0 ? element.ActualHeight : 600;

        System.Diagnostics.Debug.WriteLine($"Creating print content from element: {elementWidth}x{elementHeight}");

        // Create a Viewbox that scales the content to fit the page
        // This ensures no empty space on the right by scaling content proportionally
        System.Windows.Controls.Viewbox viewbox = new()
        {
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        // Create element container
        Grid elementContainer = new()
        {
            Width = elementWidth,
            Height = elementHeight,
            Background = Brushes.White,
        };

        // Capture element with VisualBrush
        System.Windows.Shapes.Rectangle rect = new()
        {
            Width = elementWidth,
            Height = elementHeight,
            Fill = new VisualBrush(element)
            {
                Stretch = Stretch.Fill,
            },
        };

        elementContainer.Children.Add(rect);
        viewbox.Child = elementContainer;

        System.Diagnostics.Debug.WriteLine($"Print content created - Element: {elementWidth}x{elementHeight}");

        return viewbox;
    }
}
