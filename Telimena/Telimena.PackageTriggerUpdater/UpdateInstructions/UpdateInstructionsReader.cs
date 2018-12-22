using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Telimena.PackageTriggerUpdater
{
    internal static class UpdateInstructionsReader
    {
        public static UpdateInstructions Read(FileInfo instructionFile)
        {
            try
            {
                XDocument xDoc = XDocument.Load(instructionFile.FullName);
                return DeserializeDocument(xDoc);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while reading update instructions.\r\nDetails:\r\n" + ex);
                Console.ReadKey();
                return null;
            }
        }

        internal static UpdateInstructions DeserializeDocument(XDocument xDoc)
        {
            using (XmlReader reader = xDoc.CreateReader())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(UpdateInstructions));
                return (UpdateInstructions)serializer.Deserialize(reader);
            }
        }

    }
    
}