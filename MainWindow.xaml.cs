using System.Text;
using System.Windows;
using Msfs.ControllerVisualizer.ViewModels;

namespace Msfs.ControllerVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}