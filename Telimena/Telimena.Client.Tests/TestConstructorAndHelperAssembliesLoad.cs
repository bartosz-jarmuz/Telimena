// -----------------------------------------------------------------------
//  <copyright file="ClientTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace TelimenaClient.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestConstructorAndHelperAssembliesLoad
    {
        private readonly Guid TestTelemetryKey = Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95");
        [Test]
        public void TestInitialize_AssemblyParameter()
        {
            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(new TelimenaStartupInfo(this.TestTelemetryKey){MainAssembly = this.GetType().Assembly});
            Assert.AreEqual(this.TestTelemetryKey, telimena.TelemetryKey);
            Assert.AreEqual("Telimena.Client.Tests", telimena.StaticProgramInfo.Name);
            Assert.AreEqual("Telimena.Client.Tests", telimena.StaticProgramInfo.PrimaryAssembly.Name);
            Assert.IsNotNull(telimena.StaticProgramInfo.PrimaryAssembly.VersionData.AssemblyVersion);
            Assert.IsNotNull(telimena.StaticProgramInfo.PrimaryAssembly.VersionData.FileVersion);

            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
        }

        [Test]
        public void TestInitialize_LoadHelperAssemblies_ByAssembly()
        {
            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(new TelimenaStartupInfo(this.TestTelemetryKey));
            Assert.AreEqual(this.TestTelemetryKey, telimena.TelemetryKey);

            telimena.LoadHelperAssemblies(this.GetType().Assembly, typeof(Capture).Assembly);
            Assert.AreEqual(2, telimena.StaticProgramInfo.HelperAssemblies.Count);
            Assert.AreEqual(1, telimena.StaticProgramInfo.HelperAssemblies.Count(x => x.Name == "Telimena.Client.Tests"));
            Assert.AreEqual(1, telimena.StaticProgramInfo.HelperAssemblies.Count(x => x.Name == "Moq"));
            Assert.IsTrue(telimena.StaticProgramInfo.HelperAssemblies.All(x => x.VersionData.AssemblyVersion != null  && x.VersionData.FileVersion != null && x.Name != null));
        }

        [Test]
        public void TestInitialize_LoadHelperAssemblies_ByString()
        {
            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(new TelimenaStartupInfo(this.TestTelemetryKey));
            telimena.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");
            Assert.AreEqual(2, telimena.StaticProgramInfo.HelperAssemblies.Count);
            Assert.AreEqual(1, telimena.StaticProgramInfo.HelperAssemblies.Count(x => x.Name == "Telimena.Client.Tests"));
            Assert.AreEqual(1, telimena.StaticProgramInfo.HelperAssemblies.Count(x => x.Name == "Moq"));
            Assert.IsTrue(telimena.StaticProgramInfo.HelperAssemblies.All(x => x.VersionData.AssemblyVersion != null && x.VersionData.FileVersion != null && x.Name != null));
        }

        [Test]
        public void TestInitialize_NoParameters()
        {
            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(new TelimenaStartupInfo(this.TestTelemetryKey));
            Assert.AreEqual("Telimena.Client.Tests", telimena.StaticProgramInfo.Name);
            Assert.AreEqual("Telimena.Client.Tests", telimena.StaticProgramInfo.PrimaryAssembly.Name);
            Assert.IsTrue(telimena.StaticProgramInfo.PrimaryAssemblyPath.EndsWith(@"\Telimena.Client.Tests.dll"));
            Assert.IsNotNull(telimena.StaticProgramInfo.PrimaryAssembly.VersionData.AssemblyVersion);
            Assert.IsNotNull(telimena.StaticProgramInfo.PrimaryAssembly.VersionData.FileVersion);
            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
        }


        [Test]
        public void TestInitialize_ProgramInfo()
        {
            ProgramInfo pi = new ProgramInfo {Name = "An App!", PrimaryAssembly = new AssemblyInfo(typeof(Mock).Assembly)};
            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(new TelimenaStartupInfo(Guid.Empty){ProgramInfo = pi});
            Assert.AreEqual("An App!", telimena.StaticProgramInfo.Name);
            Assert.AreEqual("Moq", telimena.StaticProgramInfo.PrimaryAssembly.Name);
            Assert.IsTrue(telimena.StaticProgramInfo.PrimaryAssemblyPath.EndsWith(@"\Moq.dll"));
            Assert.IsNotNull(telimena.StaticProgramInfo.PrimaryAssembly.VersionData.AssemblyVersion);
            Assert.IsNotNull(telimena.StaticProgramInfo.PrimaryAssembly.VersionData.FileVersion);
            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
        }
    }
}