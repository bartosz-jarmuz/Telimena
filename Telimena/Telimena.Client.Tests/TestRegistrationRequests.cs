// -----------------------------------------------------------------------
//  <copyright file="TestRegistrationRequests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using DotNetLittleHelpers;
using NUnit.Framework;

namespace TelimenaClient.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestRegistrationRequests
    {
        private readonly Guid TestTelemetryKey = Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95");

        public void Test_RegistrationFunc(TelimenaClient.Telimena telimena, Func<TelemetryInitializeResponse> func, bool skipFlagExpectedValue)
        {
            try
            {
                TelemetryInitializeResponse result = func(); 
                Assert.Fail("Exception expected");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreEqual("An error occured while posting to [api/Statistics/RegisterClient]", ex.InnerExceptions[0].Message);
                TelemetryInitializeRequest jObj = ex.RequestObjects[0].Value as TelemetryInitializeRequest;
                Assert.AreEqual(skipFlagExpectedValue, jObj.SkipUsageIncrementation);
                telimena.StaticProgramInfo.ThrowIfPublicPropertiesNotEqual(jObj.ProgramInfo, true);
            }
        }

        [Test]
        public void Test_InitializeRequestCreation()
        {
            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(this.TestTelemetryKey);
            telimena.SuppressAllErrors = false;
            Assert.AreEqual(this.TestTelemetryKey, telimena.StaticProgramInfo.TelemetryKey);
            telimena.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");
            Helpers.SetupMockHttpClient(telimena, Helpers.GetMockClient());
            this.Test_RegistrationFunc(telimena, () => telimena.InitializeAsync().GetAwaiter().GetResult(), false);
            this.Test_RegistrationFunc(telimena, () => telimena.InitializeBlocking(), false);
        }

        [Test]
        public void Test_RegisterRequestCreation()
        {
            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(this.TestTelemetryKey);
            telimena.SuppressAllErrors = false;
            telimena.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");
            Helpers.SetupMockHttpClient(telimena, Helpers.GetMockClient());
            this.Test_RegistrationFunc(telimena, () => telimena.RegisterClient().GetAwaiter().GetResult(), false);
            this.Test_RegistrationFunc(telimena, () => telimena.RegisterClient(true).GetAwaiter().GetResult(), true);
        }
    }
}