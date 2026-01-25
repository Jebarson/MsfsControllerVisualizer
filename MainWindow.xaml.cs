namespace Msfs.ControllerVisualizer;

using System.Text;
using System.Windows;
using Msfs.ControllerVisualizer.ViewModels;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        this.DataContext = new MainViewModel();
    }

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