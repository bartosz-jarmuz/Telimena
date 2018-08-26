using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Telimena.Updater
{
    internal static class UpdateInstructionsReader
    {
        public static UpdateInstructions Read(FileInfo instructionFile)
        {
            try
            {
                XDocument xDoc = XDocument.Load(instructionFile.FullName);
                return ParseDocument(xDoc);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while reading update instructions.\r\nDetails:\r\n" + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        internal static UpdateInstructions ParseDocument(XDocument xDoc)
        {
            var instructions = new UpdateInstructions();
            ProcessPackagesSection(xDoc, instructions);
            ProcessMetadataSection(xDoc, instructions);
            return instructions;
        }

        private static void ProcessMetadataSection(XDocument xDoc, UpdateInstructions instructions)
        {
            var section = xDoc.Root.Element("Metadata");
            if (section != null)
            {
                instructions.LatestVersion = section.Element("LatestVersion")?.Value;
                instructions.ProgramExecutableLocation = section.Element("ProgramExecutableLocation")?.Value;
            }
        }

        private static void ProcessPackagesSection(XDocument xDoc, UpdateInstructions instructions)
        {
            var packagesSection = xDoc.Root.Element("PackagesToInstall");
            if (packagesSection != null)
            {
                instructions.PackagePaths = new List<string>();
                foreach (XElement xElement in packagesSection.Elements())
                {
                    instructions.PackagePaths.Add(xElement.Value);
                }
            }
        }
    }
}
