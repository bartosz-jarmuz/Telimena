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
            ITelimena telimena = Telimena.Construct(new TelimenaStartupInfo(this.TestTelemetryKey){MainAssembly = this.GetType().Assembly});
            Assert.AreEqual(this.TestTelemetryKey, telimena.Properties.TelemetryKey);
            Assert.AreEqual("Telimena.Client.Tests", telimena.Properties.StaticProgramInfo.Name);
            Assert.AreEqual("Telimena.Client.Tests", telimena.Properties.StaticProgramInfo.PrimaryAssembly.Name);
            Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.AssemblyVersion);
            Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.FileVersion);

            Assert.IsNotNull(telimena.Properties.UserInfo.UserName);
            Assert.IsNotNull(telimena.Properties.UserInfo.MachineName);
        }

        [Test]
        public void TestInitialize_LoadHelperAssemblies_ByAssembly()
        {
            var si = new TelimenaStartupInfo(this.TestTelemetryKey);
            si.LoadHelperAssemblies(this.GetType().Assembly, typeof(Capture).Assembly);
            var telimena = Telimena.Construct(si);
            Assert.AreEqual(this.TestTelemetryKey, telimena.Properties.TelemetryKey);

            Assert.AreEqual(2, telimena.Properties.StaticProgramInfo.HelperAssemblies.Count);
            Assert.AreEqual(1, telimena.Properties.StaticProgramInfo.HelperAssemblies.Count(x => x.Name == "Telimena.Client.Tests"));
            Assert.AreEqual(1, telimena.Properties.StaticProgramInfo.HelperAssemblies.Count(x => x.Name == "Moq"));
            Assert.IsTrue(telimena.Properties.StaticProgramInfo.HelperAssemblies.All(x => x.VersionData.AssemblyVersion != null  && x.VersionData.FileVersion != null && x.Name != null));
        }

        [Test]
        public void TestInitialize_LoadHelperAssemblies_ByString()
        {
            var si = new TelimenaStartupInfo(this.TestTelemetryKey);
            si.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");

            var telimena = Telimena.Construct(si);
            Assert.AreEqual(2, telimena.Properties.StaticProgramInfo.HelperAssemblies.Count);
            Assert.AreEqual(1, telimena.Properties.StaticProgramInfo.HelperAssemblies.Count(x => x.Name == "Telimena.Client.Tests"));
            Assert.AreEqual(1, telimena.Properties.StaticProgramInfo.HelperAssemblies.Count(x => x.Name == "Moq"));
            Assert.IsTrue(telimena.Properties.StaticProgramInfo.HelperAssemblies.All(x => x.VersionData.AssemblyVersion != null && x.VersionData.FileVersion != null && x.Name != null));
        }

        [Test]
        public void TestInitialize_NoParameters()
        {
            var telimena = Telimena.Construct(new TelimenaStartupInfo(this.TestTelemetryKey));
            Assert.AreEqual("Telimena.Client.Tests", telimena.Properties.StaticProgramInfo.Name);
            Assert.AreEqual("Telimena.Client.Tests", telimena.Properties.StaticProgramInfo.PrimaryAssembly.Name);
            Assert.IsTrue(telimena.Properties.StaticProgramInfo.PrimaryAssemblyPath.EndsWith(@"\Telimena.Client.Tests.dll"));
            Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.AssemblyVersion);
            Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.FileVersion);
            Assert.IsNotNull(telimena.Properties.UserInfo.UserName);
            Assert.IsNotNull(telimena.Properties.UserInfo.MachineName);
        }


        [Test]
        public void TestInitialize_ProgramInfo()
        {
            ProgramInfo pi = new ProgramInfo {Name = "An App!", PrimaryAssembly = new AssemblyInfo(typeof(Mock).Assembly)};
            ITelimena telimena = Telimena.Construct(new TelimenaStartupInfo(Guid.Empty){ProgramInfo = pi});
            Assert.AreEqual("An App!", telimena.Properties.StaticProgramInfo.Name);
            Assert.AreEqual("Moq", telimena.Properties.StaticProgramInfo.PrimaryAssembly.Name);
            Assert.IsTrue(telimena.Properties.StaticProgramInfo.PrimaryAssemblyPath.EndsWith(@"\Moq.dll"));
            Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.AssemblyVersion);
            Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.FileVersion);
            Assert.IsNotNull(telimena.Properties.UserInfo.UserName);
            Assert.IsNotNull(telimena.Properties.UserInfo.MachineName);
        }
    }
}