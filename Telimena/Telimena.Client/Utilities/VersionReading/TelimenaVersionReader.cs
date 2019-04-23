using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TelimenaClient
{
    /// <summary>
    ///     One place that reads the version strings consistently for Telimena-based apps
    /// </summary>
    public static class TelimenaVersionReader
    {
        /// <summary>
        /// Reads the version of the Telimena related assemblies (enforces coherent version recognition approach)
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadToolkitVersion(string filePath)
        {
            return Read(new FileInfo(filePath), VersionTypes.FileVersion);
        }

        /// <summary>
        /// Reads the version of the Telimena related assemblies (enforces coherent version recognition approach)
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string ReadToolkitVersion(Assembly assembly)
        {
            return Read(assembly, VersionTypes.FileVersion);
        }

        /// <summary>
        ///     Returns the version string of the specified type
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="versionType"></param>
        /// <returns></returns>
        public static string Read(string filePath, VersionTypes versionType)
        {
            return Read(new FileInfo(filePath), versionType);
        }

        /// <summary>
        ///     Returns the version string of the specified type
        /// </summary>
        /// <param name="file"></param>
        /// <param name="versionType"></param>
        /// <returns></returns>
        public static string Read(FileInfo file, VersionTypes versionType)
        {
            return GetVersionFromFile(file, versionType);
        }

        /// <summary>
        ///     Returns the version string of the specified type
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="versionType"></param>
        /// <returns></returns>
        public static string Read(Assembly assembly, VersionTypes versionType)
        {
            return GetVersionFromAssembly(assembly, versionType);
        }

        /// <summary>
        ///     Returns the version string of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="versionType"></param>
        /// <returns></returns>
        public static string Read(Type type, VersionTypes versionType)
        {
            return Read(type.Assembly, versionType);
        }

        private static string GetVersionFromAssembly(Assembly assembly, VersionTypes versionType)
        {
            switch (versionType)
            {
                case VersionTypes.AssemblyVersion:
                    return assembly.GetName().Version.ToString();
                case VersionTypes.FileVersion:
                    return GetFileVersionFromAssembly(assembly);
                default:
                    throw new ArgumentOutOfRangeException(nameof(versionType), versionType, null);
            }
        }

        private static string GetFileVersionFromAssembly(Assembly assembly)
        {
            if (!string.IsNullOrWhiteSpace(assembly.Location))
            {
                try
                {
                    return FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
                }
                catch
                {
                    //ok, try another way
                }
            }

            try
            {

                var attribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
                if (attribute != null)
                {
                    return attribute.Version;
                }

                return "0.0.0.0";
            }
            catch
            {
                return "0.0.0.0";
            }
        }


        private static string GetVersionFromFile(FileInfo file, VersionTypes versionType)
        {
            switch (versionType)
            {
                case VersionTypes.AssemblyVersion:
                    return AssemblyName.GetAssemblyName(file.FullName).Version.ToString();
                case VersionTypes.FileVersion:
                    return FileVersionInfo.GetVersionInfo(file.FullName).FileVersion;
                default:
                    throw new ArgumentOutOfRangeException(nameof(versionType), versionType, null);
            }
        }
    }
}