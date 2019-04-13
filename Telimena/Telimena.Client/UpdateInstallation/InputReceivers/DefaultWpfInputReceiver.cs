using System.Windows;
using TelimenaClient.Model;
using TelimenaClient.Model.Internal;

namespace TelimenaClient
{
    internal class DefaultWpfInputReceiver : IReceiveUserInput
    {
     
        public bool ShowInstallUpdatesNowQuestion(string maxVersion, long totalDownloadSize, LiveProgramInfo programInfo)
        {

            MessageBoxResult choice = MessageBox.Show($"An update to version {maxVersion} was downloaded.\r\nWould you like to install now?"
                , $"{programInfo.Program.Name} update installation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (choice == MessageBoxResult.Yes)
            {
                return true;
            }

            return false;
        }

        public bool ShowDownloadAndInstallUpdatesQuestion(string maxVersion, long totalDownloadSize, LiveProgramInfo programInfo)
        {

            MessageBoxResult choice = MessageBox.Show($"An update to version {maxVersion} is available\r\n" +
                                                      $"Total download size: {totalDownloadSize.GetSizeString(2)}.\r\n" +
                                                      $"Would you like to download and install now?"
                , $"{programInfo.Program.Name} update download", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (choice == MessageBoxResult.Yes)
            {
                return true;
            }

            return false;
        }
    }
}