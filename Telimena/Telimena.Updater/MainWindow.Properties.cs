using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Telimena.Updater.Annotations;

namespace Telimena.Updater
{
    public partial class MainWindow
    {
        public bool ReleaseNotesVisible
        {
            get { return this.releaseNotesVisible; }
            set
            {
                if (value == this.releaseNotesVisible) return;
                this.releaseNotesVisible = value;
                this.OnPropertyChanged();
            }
        }

        private string updateVersionInfoLabel = "1.0.0.0";
        private string releaseNotes;
        private string titleLabel;
        private bool releaseNotesVisible;

        public string ReleaseNotes
        {
            get { return this.releaseNotes; }
            set
            {
                if (this.releaseNotes == value)
                {
                    return;
                }
                this.releaseNotes = value;
                this.OnPropertyChanged();
            }
        }

        public string UpdateVersionInfoLabel
        {
            get { return this.updateVersionInfoLabel; }
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
            get { return this.titleLabel; }
            set
            {
                if (value == this.titleLabel) return;
                this.titleLabel = value;
                this.OnPropertyChanged();
            }
        }

        public UpdateInstructions Instructions { get; set; }



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}