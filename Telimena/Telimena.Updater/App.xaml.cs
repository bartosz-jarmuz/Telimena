using System.Windows;

namespace Telimena.Updater
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            UpdaterStartupSettings settings = CommandLineArgumentParser.GetSettings(e.Args);
            MainWindow mainWindow;
            if (settings == null)
            {
#if DEBUG
                mainWindow = new MainWindow(new UpdateInstructions(){LatestVersion = "1.2.3.4"});
#else
                mainWindow = new MainWindow();
#endif
            }
            else
            {
                mainWindow = new MainWindow(settings);
            }

            mainWindow.Show();
        }
    }
}