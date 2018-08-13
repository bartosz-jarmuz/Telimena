using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Telimena.Client
{
    internal class DefaultWpfInputReceiver : IReceiveUserInput
    {
        public bool ShowIncludeBetaPackagesQuestion(UpdateResponse response)
        {
            MessageBoxResult choice = MessageBox.Show(
                $"There are {response.UpdatePackagesIncludingBeta.Count} update packages available, " +
                $"however {response.UpdatePackagesIncludingBeta.Count(x => x.IsBeta)} of them are pre-release versions. " +
                $"Would you like to download the pre-release updates as well?",
                "Select updates", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (choice == MessageBoxResult.Yes)
            {
                return true;
            }

            return false;
        }

        public bool ShowInstallUpdatesNowQuestion(IEnumerable<UpdatePackageData> packagesToInstall)
        {
            string maxVersion = packagesToInstall.GetMaxVersion();

            MessageBoxResult choice = MessageBox.Show($"An update to version {maxVersion} was downloaded.\r\nWould you like to install now?");
            if (choice == MessageBoxResult.Yes)
            {
                return true;
            }

            return false;
        }
    }
}