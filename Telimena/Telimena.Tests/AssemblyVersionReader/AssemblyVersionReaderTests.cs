using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Telimena.WebApp;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Assert = NUnit.Framework.Assert;

namespace Telimena.Tests
{
    [TestFixture]
    [DeploymentItem("AssemblyVersionReader")]
        public class AssemblyVersionReaderTests
    {
        private DirectoryInfo TestFolder => new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AssemblyVersionReader"));

        [Test]
        public void Test_HappyPath()
        {
            var zip = new FileInfo(Path.Combine(TestFolder.FullName, "ProperPackage.zip"));


            var stream = File.OpenRead(zip.FullName);
            var sut = new AssemblyStreamVersionReader();
            Assert.AreEqual("1.5.0.0", sut.GetVersionFromPackage("TelimenaTestSandboxApp.exe", stream, true).GetAwaiter().GetResult());
            Assert.IsTrue(stream.CanRead);
            Assert.IsTrue(stream.CanSeek);
            Assert.AreEqual(0, stream.Position);

            Assert.AreEqual("1.0.0.1", sut.GetVersionFromPackage("Telimena.Client.dll", stream, true).GetAwaiter().GetResult());
            Assert.IsTrue(stream.CanRead);
            Assert.IsTrue(stream.CanSeek);
            Assert.AreEqual(0, stream.Position);

        }

        [Test]
        public void Test_UnhappyPath()
        {
            var zip = new FileInfo(Path.Combine(TestFolder.FullName, "ImproperPackage.zip"));


            var stream = File.OpenRead(zip.FullName);
            var sut = new AssemblyStreamVersionReader();

            Assert.That( () => sut.GetVersionFromPackage("TelimenaTestSandboxApp.exe", stream, true).GetAwaiter().GetResult(), 
                Throws.Exception.With.Message.Contains($"Failed to find the required assembly in the uploaded package. [TelimenaTestSandboxApp.exe] should be present."));
            Assert.IsTrue(stream.CanRead);
            Assert.IsTrue(stream.CanSeek);
            Assert.AreEqual(0, stream.Position);

            Assert.IsNull( sut.GetVersionFromPackage("Telimena.Client.dll", stream, false).GetAwaiter().GetResult());
            Assert.IsTrue(stream.CanRead);
            Assert.IsTrue(stream.CanSeek);
            Assert.AreEqual(0, stream.Position);

        }
    }
}