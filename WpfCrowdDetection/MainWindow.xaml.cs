using MahApps.Metro.Controls;
using System.Windows;
using WpfCrowdDetection.ViewModel;

namespace WpfCrowdDetection
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            HamburgerMenuControl.SelectedIndex = 0;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            ViewModelLocator.Cleanup();
        }
    }
}