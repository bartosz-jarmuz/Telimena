namespace Telimena.Client
{
    using System;
    using System.Linq;
    using Model;

    internal static class UpdateResponseReader
    {
        public static UpdateCheckResult ReadResponse(ProgramInfo programInfo, LatestVersionResponse latestVersionResponse)
        {
            UpdateCheckResult result = new UpdateCheckResult();

            if (CompareVersions(latestVersionResponse.PrimaryAssemblyVersion.LatestVersion, programInfo.PrimaryAssembly.Version) >= 1)
            {
                AssemblyUpdateInfo updateInfo = new AssemblyUpdateInfo(programInfo.PrimaryAssembly, latestVersionResponse.PrimaryAssemblyVersion);
                result.PrimaryAssemblyUpdateInfo = updateInfo;
            }

            if (latestVersionResponse.HelperAssemblyVersions != null)
            {

                foreach (VersionInfo latestVersionInfo in latestVersionResponse.HelperAssemblyVersions)
                {
                    AssemblyInfo currentAssembly = programInfo.HelperAssemblies.FirstOrDefault(x => x.Name == latestVersionInfo.AssemblyName);
                    if (currentAssembly == null)
                    {
                        continue;
                    }

                    if (CompareVersions(latestVersionInfo.LatestVersion, currentAssembly.Version) >= 1)
                    {
                        AssemblyUpdateInfo updateInfo = new AssemblyUpdateInfo(currentAssembly, latestVersionInfo);
                        result.HelperAssembliesToUpdate.Add(updateInfo);
                    }
                }
            }

            return result;
        }

        private static int CompareVersions(string source, string target)
        {
            try
            {
                Version sourceVersion = Version.Parse(source);
                Version targetVersion = Version.Parse(target);
                return sourceVersion.CompareTo(targetVersion);
            }
            catch (Exception)
            {
                //swallow
            }

            return 0;
        }
    }
}