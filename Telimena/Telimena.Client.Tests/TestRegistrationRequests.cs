// -----------------------------------------------------------------------
//  <copyright file="TestRegistrationRequests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using DotNetLittleHelpers;
using Moq;
using NUnit.Framework;

namespace TelimenaClient.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestRegistrationRequests
    {
        private readonly Guid testTelemetryKey = Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95");

        public void Test_RegistrationFunc(ITelimena telimena, Func<TelemetryInitializeResponse> func, bool skipFlagExpectedValue)
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
                ((Telimena)telimena).StaticProgramInfo.ThrowIfPublicPropertiesNotEqual(jObj.ProgramInfo, true);
            }
        }

        [Test]
        public void Test_InitializeRequestCreation()
        {
            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(this.testTelemetryKey);
            telimena.SuppressAllErrors = false;
            Assert.AreEqual(this.testTelemetryKey, telimena.TelemetryKey);
            telimena.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");
            Helpers.SetupMockHttpClient(telimena, Helpers.GetMockClient());
            this.Test_RegistrationFunc(telimena, () => telimena.InitializeAsync_toReDo().GetAwaiter().GetResult(), false);
            this.Test_RegistrationFunc(telimena, () => telimena.InitializeBlocking_toReDo(), false);
        }

        [Test]
        public void Test_RegisterRequestCreation_EmptyKey()
        {
            ITelimena telimena = new TelimenaClient.Telimena(Guid.Empty);
            telimena.SuppressAllErrors = false;
            telimena.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");
            Helpers.SetupMockHttpClient(telimena, Helpers.GetMockClient());
            Assert.That(()=> this.Test_RegistrationFunc(telimena, () => telimena.InitializeAsync().GetAwaiter().GetResult(), false)
            ,Throws.Exception.With.Message.Contains("Telemetry key is an empty guid."));
        }

        [Test]
        public void Test_RegisterRequestCreation()
        {
            ITelimena telimena = new TelimenaClient.Telimena(this.testTelemetryKey);
            telimena.SuppressAllErrors = false;
            telimena.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");
            Helpers.SetupMockHttpClient(telimena, Helpers.GetMockClient());
            this.Test_RegistrationFunc(telimena, () => telimena.InitializeAsync().GetAwaiter().GetResult(), false);
        //todo    this.Test_RegistrationFunc(telimena, () => telimena.InitializeAsync(true).GetAwaiter().GetResult(), true);
        }
    }
}