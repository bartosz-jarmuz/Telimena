// -----------------------------------------------------------------------
//  <copyright file="ClientTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using Moq;
using NUnit.Framework;

namespace TelimenaClient.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestConstructorAndHelperAssembliesLoad
    {
        [Test]
        public void TestInitialize_AssemblyParameter()
        {
            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(this.GetType().Assembly);
            Assert.AreEqual("Telimena.Client.Tests", telimena.StaticProgramInfo.Name);
            Assert.AreEqual("Telimena.Client.Tests", telimena.StaticProgramInfo.PrimaryAssembly.Name);
            Assert.IsNotNull(telimena.StaticProgramInfo.PrimaryAssembly.Version);

            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
        }

        [Test]
        public void TestInitialize_LoadHelperAssemblies_ByAssembly()
        {
            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena();
            telimena.LoadHelperAssemblies(this.GetType().Assembly, typeof(Capture).Assembly);
            Assert.AreEqual(2, telimena.StaticProgramInfo.HelperAssemblies.Count);
            Assert.AreEqual(1, telimena.StaticProgramInfo.HelperAssemblies.Count(x => x.Name == "Telimena.Client.Tests"));
            Assert.AreEqual(1, telimena.StaticProgramInfo.HelperAssemblies.Count(x => x.Name == "Moq"));
            Assert.IsTrue(telimena.StaticProgramInfo.HelperAssemblies.All(x => x.Version != null && x.Name != null));
        }

        [Test]
        public void TestInitialize_LoadHelperAssemblies_ByString()
        {
            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena();
            telimena.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");
            Assert.AreEqual(2, telimena.StaticProgramInfo.HelperAssemblies.Count);
            Assert.AreEqual(1, telimena.StaticProgramInfo.HelperAssemblies.Count(x => x.Name == "Telimena.Client.Tests"));
            Assert.AreEqual(1, telimena.StaticProgramInfo.HelperAssemblies.Count(x => x.Name == "Moq"));
            Assert.IsTrue(telimena.StaticProgramInfo.HelperAssemblies.All(x => x.Version != null && x.Name != null));
        }

        [Test]
        public void TestInitialize_NoParameters()
        {
            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena();
            Assert.AreEqual("Telimena.Client.Tests", telimena.StaticProgramInfo.Name);
            Assert.AreEqual("Telimena.Client.Tests", telimena.StaticProgramInfo.PrimaryAssembly.Name);
            Assert.IsTrue(telimena.StaticProgramInfo.PrimaryAssemblyPath.EndsWith(@"\Telimena.Client.Tests.dll"));
            Assert.IsNotNull(telimena.StaticProgramInfo.PrimaryAssembly.Version);
            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
        }

        [Test]
        public void TestInitialize_ProgramInfo()
        {
            ProgramInfo pi = new ProgramInfo {Name = "An App!", PrimaryAssembly = new AssemblyInfo(typeof(Mock).Assembly)};
            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(pi);
            Assert.AreEqual("An App!", telimena.StaticProgramInfo.Name);
            Assert.AreEqual("Moq", telimena.StaticProgramInfo.PrimaryAssembly.Name);
            Assert.IsTrue(telimena.StaticProgramInfo.PrimaryAssemblyPath.EndsWith(@"\Moq.dll"));
            Assert.IsNotNull(telimena.StaticProgramInfo.PrimaryAssembly.Version);
            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
        }
    }
}