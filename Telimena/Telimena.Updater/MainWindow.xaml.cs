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
            this.titlePrefix = this.GetTitle();
            this.TitleLabel = this.titlePrefix;
            this.PrepareReleaseNotes();
        }

        private readonly string titlePrefix;

        private string GetTitle()
        {
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            return $"{this.Instructions.ProgramName} Updater v. {version}";
        }

        public MainWindow()
        {
            this.InitializeComponent();
            MessageBox.Show("In order to check for updates, run the main app.\r\n" + "The updater is not a standalone application.", this.titlePrefix
                , MessageBoxButton.OK, MessageBoxImage.Information);
            Application.Current.Shutdown();
        }

        public MainWindow(UpdaterStartupSettings updaterStartupSettings) : this(UpdateInstructionsReader.Read(updaterStartupSettings.InstructionsFile))
        {
           
        }

        private void PrepareReleaseNotes()
        {
            if (this.Instructions.Packages != null && this.Instructions.Packages.Any(x => !string.IsNullOrEmpty(x.ReleaseNotes)))
            {
                this.ReleaseNotesVisible = true;
                this.ReleaseNotes = string.Join("\r\n", this.Instructions.Packages.Select(x => x.ReleaseNotes));
            }
        }

        private void PerformUpdatesButton_Click(object sender, RoutedEventArgs e)
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
                MessageBox.Show($"An error occurred while updating the program.\r\n{ex}", $"Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Environment.Exit(0);
        }
    }
}