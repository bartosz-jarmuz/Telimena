using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Telimena.Updater.Annotations;

namespace Telimena.Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _updateVersionInfoLabel;
        private UpdaterStartupSettings UpdaterStartupSettings { get; }
        private FileInfo InstructionsFile { get; }

        public MainWindow()
        {
            this.InitializeComponent();
            MessageBox.Show("In order to check for updates, run the main app.\r\n" +
                            "The updater is not a standalone application.", "Telimena Updater", MessageBoxButton.OK,MessageBoxImage.Information);
            Application.Current.Shutdown();
        }

        public MainWindow(UpdaterStartupSettings updaterStartupSettings)
        {
            this.UpdaterStartupSettings = updaterStartupSettings;
            this.InstructionsFile = this.UpdaterStartupSettings?.InstructionsFile;
            this.Instructions = UpdateInstructionsReader.Read(this.InstructionsFile);
            this.InitializeComponent();
            this.UpdateVersionInfoLabel = this.Instructions.LatestVersion;
        }

        public UpdateInstructions Instructions { get; set; }

        public string UpdateVersionInfoLabel
        {
            get => this._updateVersionInfoLabel;
            set
            {
                if (value == this._updateVersionInfoLabel) return;
                this._updateVersionInfoLabel = value;
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var worker = new UpdateWorker();
            worker.PerformUpdate(this.Instructions);
            if (MessageBox.Show("Update complete. Would you like to run the app now?", "Update complete", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                Process.Start(this.Instructions.ProgramExecutableLocation);
            }
            Environment.Exit(0);
        }
    }
}
