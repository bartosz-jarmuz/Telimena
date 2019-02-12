using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using TelimenaClient.Model;

namespace TelimenaClient
{
    internal static class UpdateInstructionCreator
    {
        public static FileInfo CreateInstructionsFile(IEnumerable<UpdatePackageData> packages, ProgramInfo programInfo)
        {
            Tuple<XDocument, FileInfo> tuple = CreateXDoc(packages, programInfo);
            XDocument xDoc = tuple.Item1;
            FileInfo file = tuple.Item2;

            xDoc.Save(file.FullName);

            return file;
        }

        internal static Tuple<XDocument, FileInfo> CreateXDoc(IEnumerable<UpdatePackageData> packages, ProgramInfo programInfo)
        {
            List<UpdatePackageData> sorted = Sort(packages);
            DirectoryInfo updatesFolder = new FileInfo(sorted.First().StoredFilePath).Directory.Parent;
            FileInfo instructionsFile = new FileInfo(Path.Combine(updatesFolder.FullName, "UpdateInstructions.xml"));

            UpdateInstructions instructions = new UpdateInstructions()
            {
                LatestVersion = sorted.Last().Version,
                ProgramExecutableLocation = programInfo.PrimaryAssemblyPath,
                Packages = new List<UpdateInstructions.PackageData>(sorted.Select(data => new UpdateInstructions.PackageData()
                {
                    Version = data.Version,
                    Path = data.StoredFilePath,
                    ReleaseNotes = data.ReleaseNotes
                }))
            };

            XDocument doc = new XDocument();
            using (XmlWriter writer = doc.CreateWriter())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(UpdateInstructions));
                serializer.Serialize(writer, instructions);
            }

            return new Tuple<XDocument, FileInfo>(doc, instructionsFile);
        }

        internal static List<UpdatePackageData> Sort(IEnumerable<UpdatePackageData> packages)
        {
            return packages.OrderBy(x => x.Version, new TelimenaVersionStringComparer()).ToList();
        }

    }
}