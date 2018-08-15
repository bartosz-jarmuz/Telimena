using System.IO;
using System.Windows;

namespace Telimena.Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UpdaterSettings UpdaterSettings { get; }
        private FileInfo InstructionsFile { get; }

        public MainWindow()
        {
            this.InitializeComponent();
            MessageBox.Show("In order to check for updates, run the main app.\r\n" +
                            "The updater is not a standalone application.", "Telimena Updater", MessageBoxButton.OK,MessageBoxImage.Information);
            Application.Current.Shutdown();
        }

        public MainWindow(UpdaterSettings updaterSettings)
        {
            this.UpdaterSettings = updaterSettings;
            this.InstructionsFile = this.UpdaterSettings?.InstructionsFile;

            this.InitializeComponent();
        }
    }
}
