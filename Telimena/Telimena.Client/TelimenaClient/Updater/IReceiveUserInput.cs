using System.Collections.Generic;

namespace Telimena.Client
{
    public interface IReceiveUserInput
    {
        bool ShowIncludeBetaPackagesQuestion(UpdateResponse response);
        bool ShowInstallUpdatesNowQuestion(IEnumerable<UpdatePackageData> packagesToInstall);
    }
}