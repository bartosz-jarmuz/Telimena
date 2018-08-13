using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Telimena.Client;

namespace Telimena
{
    internal static class UpdateInstructionCreator
    {
        internal static Tuple<XDocument, FileInfo> CreateXDoc(IEnumerable<UpdatePackageData> packages)
        {
            List<UpdatePackageData> sorted = Sort(packages);
            DirectoryInfo updatesFolder = new FileInfo(sorted.First().StoredFilePath).Directory;
            FileInfo instructionsFile = new FileInfo(Path.Combine(updatesFolder.FullName, "UpdateInstructions.xml"));
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement("UpdateInstructions"));
            foreach (UpdatePackageData updatePackageData in sorted)
            {
                // ReSharper disable once PossibleNullReferenceException
                xDoc.Root.Add(new XElement("file", updatePackageData.StoredFilePath));
            }
            return new Tuple<XDocument, FileInfo>(xDoc, instructionsFile);
        }

        internal static List<UpdatePackageData> Sort(IEnumerable<UpdatePackageData> packages)
        {
            return packages.OrderByDescending(x => x.Version, new VersionStringComparer()).ToList();
        }

        public static FileInfo CreateInstructionsFile(IEnumerable<UpdatePackageData> packages)
        {
            Tuple<XDocument, FileInfo> tuple = CreateXDoc(packages);
            XDocument xDoc= tuple.Item1;
            FileInfo file = tuple.Item2;

            xDoc.Save(file.FullName);

            return file;
        }

    }
}