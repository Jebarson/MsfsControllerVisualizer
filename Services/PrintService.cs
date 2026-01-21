namespace Msfs.ControllerVisualizer.Services;

using System;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

/// <summary>
/// Provides services for printing visual elements to a printer.
/// </summary>
public class PrintService
{
    /// <summary>
    /// Opens a print dialog and prints the specified visual element.
    /// </summary>
    /// <param name="visualElement">The visual element to print (e.g., Viewbox, Canvas, UserControl).</param>
    /// <param name="documentTitle">The title of the print job.</param>
    /// <returns>True if the print was successful, false if cancelled or failed.</returns>
    public bool Print(FrameworkElement visualElement, string documentTitle = "Controller Layout")
    {
        if (visualElement == null)
        {
            return false;
        }

        string? tempPngPath = null;

        try
        {
            // If the element is a Viewbox, extract its child to avoid rendering the container
            FrameworkElement elementToRender = visualElement;
            if (visualElement is System.Windows.Controls.Viewbox viewbox && viewbox.Child is FrameworkElement child)
            {
                elementToRender = child;
            }

            // Step 1: Render the visual element to a high-resolution PNG file
            tempPngPath = this.SaveVisualAsPng(elementToRender);

            if (tempPngPath == null || !File.Exists(tempPngPath))
            {
                MessageBox.Show("Failed to create temporary image file for printing.", "Print Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"Created temporary PNG: {tempPngPath}");

            // Step 2: Create and configure the print dialog
            PrintDialog printDialog = new();

            // Set default print settings for high-quality image printing in landscape mode
            printDialog.PrintTicket.PageOrientation = PageOrientation.Landscape;
            printDialog.PrintTicket.OutputQuality = OutputQuality.Photographic;
            printDialog.PrintTicket.PageMediaSize = new(PageMediaSizeName.ISOA4);
            printDialog.PrintTicket.OutputColor = OutputColor.Color;

            // Show the Windows print dialog (includes preview on Windows 10+)
            bool? result = printDialog.ShowDialog();

            if (result == true)
            {
                // Step 3: Load the PNG file and send it to the printer
                BitmapImage bitmapImage = new();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(tempPngPath);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Load fully so we can delete the file
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Make it thread-safe and release file lock

                // Get the print capabilities
                PrintCapabilities capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);

                // Get the printable area size
                Size printableSize = new(
                    capabilities.PageImageableArea.ExtentWidth,
                    capabilities.PageImageableArea.ExtentHeight);

                System.Diagnostics.Debug.WriteLine($"Printable area: {printableSize.Width}x{printableSize.Height}");

                // Define narrow margins (0.25 inches = 24 pixels at 96 DPI)
                double marginSize = 24;
                double contentWidth = printableSize.Width - (marginSize * 2);
                double contentHeight = printableSize.Height;

                // Create main container with margins
                Grid mainContainer = new()
                {
                    Width = printableSize.Width,
                    Height = printableSize.Height,
                    Background = Brushes.White
                };

                // Create a Viewbox to automatically scale content to fit within margins
                System.Windows.Controls.Viewbox printViewbox = new()
                {
                    Width = contentWidth,
                    Height = contentHeight,
                    Stretch = Stretch.Uniform,
                    StretchDirection = StretchDirection.Both,
                    Margin = new(marginSize, 0, marginSize, 0)
                };

                // Put the image in an Image control inside the Viewbox
                Image printImage = new()
                {
                    Source = bitmapImage,
                    Stretch = Stretch.Uniform
                };

                printViewbox.Child = printImage;
                mainContainer.Children.Add(printViewbox);

                // Measure and arrange the container
                mainContainer.Measure(printableSize);
                mainContainer.Arrange(new(0, 0, printableSize.Width, printableSize.Height));
                mainContainer.UpdateLayout();

                // Send to printer
                printDialog.PrintVisual(mainContainer, documentTitle);

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Print error: {ex.Message}");
            MessageBox.Show($"Failed to print: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        finally
        {
            // Clean up temporary PNG file
            if (tempPngPath != null && File.Exists(tempPngPath))
            {
                try
                {
                    File.Delete(tempPngPath);
                    System.Diagnostics.Debug.WriteLine($"Deleted temporary PNG: {tempPngPath}");
                }
                catch (Exception deleteEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Could not delete temporary file: {deleteEx.Message}");
                    // Not critical if we can't delete it
                }
            }
        }
    }

    /// <summary>
    /// Saves a visual element as a high-resolution PNG file to a temporary location.
    /// </summary>
    /// <param name="element">The framework element to save.</param>
    /// <returns>Path to the temporary PNG file, or null if failed.</returns>
    private string? SaveVisualAsPng(FrameworkElement element)
    {
        try
        {
            // Get the rendered size from the element
            double width = element.ActualWidth;
            double height = element.ActualHeight;

            // If no size, try RenderSize
            if (width == 0 || height == 0)
            {
                width = element.RenderSize.Width;
                height = element.RenderSize.Height;
            }

            // If still no size, use default
            if (width == 0 || height == 0)
            {
                width = 1200;
                height = 650;
            }

            System.Diagnostics.Debug.WriteLine($"Rendering element with size: {width}x{height}");

            // Use 600 DPI for maximum quality printing (professional print quality)
            // PNG supports arbitrary DPI values - 600 provides excellent quality without excessive file size
            double dpi = 600;

            // Calculate scaled dimensions for higher DPI
            double scale = dpi / 96.0;
            int pixelWidth = (int)Math.Ceiling(width * scale);
            int pixelHeight = (int)Math.Ceiling(height * scale);

            System.Diagnostics.Debug.WriteLine($"Creating PNG at {dpi} DPI: {pixelWidth}x{pixelHeight} pixels");

            // Create a Grid container with white background
            Grid container = new()
            {
                Width = width,
                Height = height,
                Background = Brushes.White
            };

            // Create a Rectangle with VisualBrush to capture the element
            System.Windows.Shapes.Rectangle rect = new()
            {
                Width = width,
                Height = height,
                Fill = new VisualBrush(element)
            };

            container.Children.Add(rect);

            // Measure and arrange the container
            container.Measure(new(width, height));
            container.Arrange(new(0, 0, width, height));
            container.UpdateLayout();

            // Create the render target bitmap at high DPI
            RenderTargetBitmap bitmap = new(
                pixelWidth,
                pixelHeight,
                dpi,
                dpi,
                PixelFormats.Pbgra32);

            // Render the container
            bitmap.Render(container);

            // Create PNG encoder
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            // Save to temporary file
            string tempPath = Path.Combine(Path.GetTempPath(), $"ControllerLayout_{Guid.NewGuid()}.png");

            using (FileStream fileStream = new(tempPath, FileMode.Create, FileAccess.Write))
            {
                encoder.Save(fileStream);
            }

            System.Diagnostics.Debug.WriteLine($"Saved PNG to: {tempPath}");

            return tempPath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving visual as PNG: {ex.Message}");
            MessageBox.Show($"Failed to create image: {ex.Message}", "Print Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

}
