// -----------------------------------------------------------------------
//  <copyright file="ClientTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using Moq;
using NUnit.Framework;
using Telimena.ToolkitClient;

namespace Telimena.Client.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestConstructorAndHelperAssembliesLoad
    {
        [Test]
        public void TestInitialize_AssemblyParameter()
        {
            ToolkitClient.Telimena telimena = new ToolkitClient.Telimena(this.GetType().Assembly);
            Assert.AreEqual("Telimena.Client.Tests", telimena.ProgramInfo.Name);
            Assert.AreEqual("Telimena.Client.Tests", telimena.ProgramInfo.PrimaryAssembly.Name);
            Assert.IsNotNull(telimena.ProgramInfo.PrimaryAssembly.Version);

            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
        }

        [Test]
        public void TestInitialize_LoadHelperAssemblies_ByAssembly()
        {
            ToolkitClient.Telimena telimena = new ToolkitClient.Telimena();
            telimena.LoadHelperAssemblies(this.GetType().Assembly, typeof(Capture).Assembly);
            Assert.AreEqual(2, telimena.ProgramInfo.HelperAssemblies.Count);
            Assert.AreEqual(1, telimena.ProgramInfo.HelperAssemblies.Count(x => x.Name == "Telimena.Client.Tests"));
            Assert.AreEqual(1, telimena.ProgramInfo.HelperAssemblies.Count(x => x.Name == "Moq"));
            Assert.IsTrue(telimena.ProgramInfo.HelperAssemblies.All(x => x.Version != null && x.Name != null));
        }

        [Test]
        public void TestInitialize_LoadHelperAssemblies_ByString()
        {
            ToolkitClient.Telimena telimena = new ToolkitClient.Telimena();
            telimena.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");
            Assert.AreEqual(2, telimena.ProgramInfo.HelperAssemblies.Count);
            Assert.AreEqual(1, telimena.ProgramInfo.HelperAssemblies.Count(x => x.Name == "Telimena.Client.Tests"));
            Assert.AreEqual(1, telimena.ProgramInfo.HelperAssemblies.Count(x => x.Name == "Moq"));
            Assert.IsTrue(telimena.ProgramInfo.HelperAssemblies.All(x => x.Version != null && x.Name != null));
        }

        [Test]
        public void TestInitialize_NoParameters()
        {
            ToolkitClient.Telimena telimena = new ToolkitClient.Telimena();
            Assert.AreEqual("Telimena.Client.Tests", telimena.ProgramInfo.Name);
            Assert.AreEqual("Telimena.Client.Tests", telimena.ProgramInfo.PrimaryAssembly.Name);
            Assert.IsTrue(telimena.ProgramInfo.PrimaryAssemblyPath.EndsWith(@"\Telimena.Client.Tests.dll"));
            Assert.IsNotNull(telimena.ProgramInfo.PrimaryAssembly.Version);
            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
        }

        [Test]
        public void TestInitialize_ProgramInfo()
        {
            ProgramInfo pi = new ProgramInfo {Name = "An App!", PrimaryAssembly = new AssemblyInfo(typeof(Mock).Assembly)};
            ToolkitClient.Telimena telimena = new ToolkitClient.Telimena(pi);
            Assert.AreEqual("An App!", telimena.ProgramInfo.Name);
            Assert.AreEqual("Moq", telimena.ProgramInfo.PrimaryAssembly.Name);
            Assert.IsTrue(telimena.ProgramInfo.PrimaryAssemblyPath.EndsWith(@"\Moq.dll"));
            Assert.IsNotNull(telimena.ProgramInfo.PrimaryAssembly.Version);
            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
        }
    }
}