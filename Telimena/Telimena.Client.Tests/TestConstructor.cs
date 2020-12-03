// -----------------------------------------------------------------------
//  <copyright file="ClientTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Reflection;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using TelimenaClient.Model;
using TelimenaClient.Model.Internal;

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
            ITelimena telimena;
            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                {
                    telimena = TelimenaFactory.Construct(new TelimenaStartupInfo(this.TestTelemetryKey, Helpers.TeliUri) { MainAssembly = this.GetType().Assembly });
                }
                else
                {
                    telimena = Telimena.Construct(new TelimenaStartupInfo(this.TestTelemetryKey, Helpers.TeliUri) { MainAssembly = this.GetType().Assembly });
                }

                Assert.AreEqual(this.TestTelemetryKey, telimena.Properties.TelemetryKey);
                Assert.AreEqual("Telimena.Client.Tests", telimena.Properties.StaticProgramInfo.Name);
                Assert.AreEqual("Telimena.Client.Tests", telimena.Properties.StaticProgramInfo.PrimaryAssembly.Name);
                Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.AssemblyVersion);
                Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.FileVersion);

            }


        }

        [Test]
        public void TestInitialize_NoParameters()
        {
            ITelimena telimena;
            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                {
                    telimena = TelimenaFactory.Construct(new TelimenaStartupInfo(this.TestTelemetryKey, Helpers.TeliUri));
                }
                else
                {
                    telimena = Telimena.Construct(new TelimenaStartupInfo(this.TestTelemetryKey, Helpers.TeliUri));
                }

                Assert.AreEqual("Telimena.Client.Tests", telimena.Properties.StaticProgramInfo.Name);
                Assert.AreEqual("Telimena.Client.Tests", telimena.Properties.StaticProgramInfo.PrimaryAssembly.Name);
                Assert.IsTrue(
                    telimena.Properties.StaticProgramInfo.PrimaryAssemblyPath.EndsWith(@"\Telimena.Client.Tests.dll"));
                Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.AssemblyVersion);
                Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.FileVersion);
        
            }
        }


        [Test]
        public void TestInitialize_ProgramInfo()
        {
            ProgramInfo pi = new ProgramInfo { Name = "An App!", PrimaryAssembly = new Model.AssemblyInfo(typeof(Mock).Assembly) };

            ITelimena telimena;
            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                {
                     telimena = TelimenaFactory.Construct(new TelimenaStartupInfo(Guid.Empty, Helpers.TeliUri) { ProgramInfo = pi });
                }
                else
                {
                    telimena = Telimena.Construct(new TelimenaStartupInfo(Guid.Empty, Helpers.TeliUri) { ProgramInfo = pi });
                }
                Assert.AreEqual("An App!", telimena.Properties.StaticProgramInfo.Name);
                Assert.AreEqual("Moq", telimena.Properties.StaticProgramInfo.PrimaryAssembly.Name);
                Assert.IsTrue(telimena.Properties.StaticProgramInfo.PrimaryAssemblyPath.EndsWith(@"\Moq.dll"));
                Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.AssemblyVersion);
                Assert.IsNotNull(telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData.FileVersion);
 
            }
            
        }

        [Test]
        public void TestNullObject_AssertItDoesNotExplode()
        {
            var teli = Telimena.Construct(new ExplodingITelimenaStartupInfo());
            this.ValidateFaultyTeli(teli);

            teli = TelimenaFactory.Construct(new ExplodingITelimenaStartupInfo());
            this.ValidateFaultyTeli(teli);




        }

        private void ValidateFaultyTeli(ITelimena teli)
        {
            Assert.IsTrue(teli.GetType() == typeof(NullObjectTelimena));
            teli.Track.Event("There should be no error here even though it is not working");
            teli.Track.View("There should be no error here even though it is not working");
            teli.Track.Log(LogLevel.Warn, "There should be no error here even though it is not working");
            teli.Track.SendAllDataNow();

            UpdateCheckResult result = teli.Update.CheckForUpdates();

            Assert.AreEqual("Update check handled by NullObjectTelemetryModule", result.Exception.Message);
            UpdateInstallationResult result2 = teli.Update.HandleUpdatesAsync(true).Result;

            Assert.IsNotNull(teli.Properties);
        }


        private class ExplodingITelimenaStartupInfo : ITelimenaStartupInfo
        {
            public Guid TelemetryKey { get; }
            public Uri TelemetryApiBaseUrl { get; }
            public Assembly MainAssembly => throw new InvalidOperationException("BOOM");
            public ProgramInfo ProgramInfo { get; }
            public UserInfo UserInfo { get; }
            public bool SuppressAllErrors { get; }
            public string InstrumentationKey { get; }
            public bool RegisterUnhandledExceptionsTracking { get; }
            public TimeSpan DeliveryInterval { get; set; }

        }

    }
}