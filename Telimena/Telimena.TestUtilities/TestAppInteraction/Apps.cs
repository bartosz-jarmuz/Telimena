using System;
using System.IO;

namespace Telimena.TestUtilities.Base.TestAppInteraction
{
    public static class Apps
    {
        public static class ProductCodes
        {
            public static Guid InstallersTestAppMsi3V1 { get; } = Guid.Parse("e98b7c60-184f-4636-b870-fd4658e72a58");
            public static Guid InstallersTestAppMsi3V2 { get; } = Guid.Parse("8a33af7c-a8df-4321-b4c5-c5fe0b48b1ec");
        }

        public static class Names
        {
            public static string AutomaticTestsClient { get; } = "AutomaticTestsClient";
            public static string PackageUpdaterTest { get; } = "PackageTriggerUpdaterTestApp";

            public static string InstallersTestAppMsi3 { get; } = "InstallersTestAppMsi3";
            public static string User3TestApp { get; } = "User3TestApp";
        }

        public static class PackageNames
        {
            public static string AutomaticTestsClientAppV1 { get; } = "TestApp v1.0.0.0.zip";
            public static string AutomaticTestsClientAppV2 { get; } = "AutomaticTestsClientv2.zip";
            public static string AutomaticTestsClientAppV3Beta { get; } = "AutomaticTestsClientv3Beta.zip";
            public static string InstallersTestAppMsi3V1 { get; } = "v1.1.2.9_InstallersTestApp.Msi3Installer.msi";
            public static string InstallersTestAppMsi3V2 { get; } = "v2.1.2.9_InstallersTestApp.Msi3Installer.msi";
            public static string PackageUpdaterTestAppV1 { get; } = "PackageTriggerUpdaterTestApp v.1.0.0.0.zip";
            public static string EmbeddedAssemblyTestApp { get; } = "EmbeddedAssemblyTestApp.zip";
        }

        public static class Keys
        {
            public static Guid AutomaticTestsClient { get; } = Guid.Parse("efacd375-d746-48de-9882-b7ca4426d1e2");
            public static Guid PackageUpdaterClient { get; } = Guid.Parse("43808405-afca-4abb-a92a-519489d62290");
            public static Guid InstallersTestAppMsi3 { get; } = Guid.Parse("637df89e-50bc-4bb2-aa90-1c664b056510");
            public static Guid User3TestApp { get; } = Guid.Parse("24c34206-0486-4251-91d6-c5314b720f61");
        }

        public static class Paths
        {
            public static FileInfo InstallersTestAppMsi3 =>
                new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "TelimenaTestsApps"
                    , "InstallersTestApp.Msi3Installer App", "InstallersTestApp.exe"));
        }
    }
}