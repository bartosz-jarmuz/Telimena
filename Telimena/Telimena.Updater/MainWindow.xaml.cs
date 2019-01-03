using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using Telimena.Updater.Annotations;
using AssemblyLoadEventArgs = System.AssemblyLoadEventArgs;

namespace Telimena.Updater
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        internal MainWindow(UpdateInstructions instructions)
        {
            this.Instructions = instructions;
            this.InitializeComponent();
            this.UpdateVersionInfoLabel = this.Instructions.LatestVersion;
            this.TitleLabel = "Updater v. " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        public MainWindow()
        {
            this.InitializeComponent();
            MessageBox.Show("In order to check for updates, run the main app.\r\n" + "The updater is not a standalone application.", "Telimena Updater"
                , MessageBoxButton.OK, MessageBoxImage.Information);
            Application.Current.Shutdown();
        }

        public MainWindow(UpdaterStartupSettings updaterStartupSettings) : this(UpdateInstructionsReader.Read(updaterStartupSettings.InstructionsFile))
        {
           
        }

        private string updateVersionInfoLabel;
        private string titleLabel;

        public UpdateInstructions Instructions { get; set; }

        public string UpdateVersionInfoLabel
        {
            get => this.updateVersionInfoLabel;
            set
            {
                if (value == this.updateVersionInfoLabel)
                {
                    return;
                }

                this.updateVersionInfoLabel = value;
                this.OnPropertyChanged();
            }
        }

        public string TitleLabel
        {
            get => this.titleLabel;
            set
            {
                if (value == this.titleLabel) return;
                this.titleLabel = value;
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
            UpdateWorker worker = new UpdateWorker();
            try
            {
                if (this.Instructions == null)
                {
                    MessageBox.Show($"Cannot perform update. Problem with instructions.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    ProcessKiller.Kill(this.Instructions.ProgramExecutableLocation);

                    worker.PerformUpdate(this.Instructions);
                    if (MessageBox.Show("Update complete. Would you like to run the app now?", "Update complete", MessageBoxButton.YesNo
                            , MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        Process.Start(this.Instructions.ProgramExecutableLocation);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating the program.\r\n{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Environment.Exit(0);

        }
    }
}