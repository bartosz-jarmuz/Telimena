using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Telimena.Client
{

    

    internal static class UpdateInstructionCreator
    {
        internal static Tuple<XDocument, FileInfo> CreateXDoc(IEnumerable<UpdatePackageData> packages, ProgramInfo programInfo)
        {
            List<UpdatePackageData> sorted = Sort(packages);
            DirectoryInfo updatesFolder = new FileInfo(sorted.First().StoredFilePath).Directory;
            FileInfo instructionsFile = new FileInfo(Path.Combine(updatesFolder.FullName, "UpdateInstructions.xml"));
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement("UpdateInstructions"));
            InsertPackagesData(xDoc, sorted);
            InsertMetadata(xDoc, sorted, programInfo);
            return new Tuple<XDocument, FileInfo>(xDoc, instructionsFile);
        }

        private static void InsertMetadata(XDocument xDoc, List<UpdatePackageData> sorted, ProgramInfo programInfo)
        {
            var metadata = new XElement("Metadata");
            xDoc.Root.Add(metadata);
            var newest = sorted.Last();
            metadata.Add(new XElement("LatestVersion", newest.Version));
            metadata.Add(new XElement("ProgramExecutableLocation", programInfo.PrimaryAssemblyPath));
        }

        private static void InsertPackagesData(XDocument xDoc, List<UpdatePackageData> sorted)
        {
            var packagesToInstall = new XElement("PackagesToInstall");
            xDoc.Root.Add(packagesToInstall);
            foreach (UpdatePackageData updatePackageData in sorted)
            {
                // ReSharper disable once PossibleNullReferenceException
                packagesToInstall.Add(new XElement("File", updatePackageData.StoredFilePath));
            }
        }

        internal static List<UpdatePackageData> Sort(IEnumerable<UpdatePackageData> packages)
        {
            return packages.OrderBy(x => x.Version, new TelimenaVersionStringComparer()).ToList();
        }

        public static FileInfo CreateInstructionsFile(IEnumerable<UpdatePackageData> packages, ProgramInfo programInfo)
        {
            Tuple<XDocument, FileInfo> tuple = CreateXDoc(packages, programInfo);
            XDocument xDoc= tuple.Item1;
            FileInfo file = tuple.Item2;

            xDoc.Save(file.FullName);

            return file;
        }

    }
}