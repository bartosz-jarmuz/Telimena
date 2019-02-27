// -----------------------------------------------------------------------
//  <copyright file="ClientTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using TelimenaClient.Model;

namespace TelimenaClient.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestConstructor
    {
        private readonly Guid TestTelemetryKey = Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95");
        [Test]
        public void TestInitialize_AssemblyParameter()
        {
            ITelimena telimena = TelimenaFactory.Construct(new TelimenaStartupInfo(this.TestTelemetryKey){MainAssembly = this.GetType().Assembly});
            Assert.AreEqual(this.TestTelemetryKey, telimena.Properties.TelemetryKey);
            Assert.AreEqual("Telimena.Client.Tests", telimena.Properties.StaticProgramInfo.Name);
            Assert.AreEqual("Telimena.Client.Tests", telimena.Properties.StaticProgramInfo.PrimaryAssembly.Name);
            Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.AssemblyVersion);
            Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.FileVersion);

            Assert.IsNotNull(telimena.Properties.UserInfo.UserIdentifier);
            Assert.IsNotNull(telimena.Properties.UserInfo.MachineName);
        }

        [Test]
        public void TestInitialize_NoParameters()
        {
            var telimena = TelimenaFactory.Construct(new TelimenaStartupInfo(this.TestTelemetryKey));
            Assert.AreEqual("Telimena.Client.Tests", telimena.Properties.StaticProgramInfo.Name);
            Assert.AreEqual("Telimena.Client.Tests", telimena.Properties.StaticProgramInfo.PrimaryAssembly.Name);
            Assert.IsTrue(telimena.Properties.StaticProgramInfo.PrimaryAssemblyPath.EndsWith(@"\Telimena.Client.Tests.dll"));
            Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.AssemblyVersion);
            Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.FileVersion);
            Assert.IsNotNull(telimena.Properties.UserInfo.UserIdentifier);
            Assert.IsNotNull(telimena.Properties.UserInfo.MachineName);
        }


        [Test]
        public void TestInitialize_ProgramInfo()
        {
            ProgramInfo pi = new ProgramInfo {Name = "An App!", PrimaryAssembly = new Model.AssemblyInfo(typeof(Mock).Assembly)};
            ITelimena telimena = TelimenaFactory.Construct(new TelimenaStartupInfo(Guid.Empty){ProgramInfo = pi});
            Assert.AreEqual("An App!", telimena.Properties.StaticProgramInfo.Name);
            Assert.AreEqual("Moq", telimena.Properties.StaticProgramInfo.PrimaryAssembly.Name);
            Assert.IsTrue(telimena.Properties.StaticProgramInfo.PrimaryAssemblyPath.EndsWith(@"\Moq.dll"));
            Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.AssemblyVersion);
            Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.FileVersion);
            Assert.IsNotNull(telimena.Properties.UserInfo.UserIdentifier);
            Assert.IsNotNull(telimena.Properties.UserInfo.MachineName);
        }
    }
}