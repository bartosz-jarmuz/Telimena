// -----------------------------------------------------------------------
//  <copyright file="TestRegistrationRequests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using DotNetLittleHelpers;
using NUnit.Framework;
using TelimenaClient.Model;

namespace TelimenaClient.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestRegistrationRequests
    {
        private readonly Guid testTelemetryKey = Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95");

        [Test]
        public void Test_InitializeRequestCreation()
        {
            TelimenaStartupInfo si = new TelimenaStartupInfo(this.testTelemetryKey);
            si.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");

            Telimena telimena = TelimenaFactory.Construct(si) as Telimena ;
            telimena.Properties.SuppressAllErrors = false;
            Assert.AreEqual(this.testTelemetryKey, telimena.Properties.TelemetryKey);
            Helpers.SetupMockHttpClient(telimena, Helpers.GetMockClient());
            this.Test_RegistrationFunc(telimena, () => telimena.Initialize().GetAwaiter().GetResult(), false);

        }

        [Test]
        public void Test_RegisterRequestCreation()
        {
            Telimena telimena = TelimenaFactory.Construct(new TelimenaStartupInfo(this.testTelemetryKey)) as Telimena;
            telimena.Properties.SuppressAllErrors = false;
            Helpers.SetupMockHttpClient(telimena, Helpers.GetMockClient());
            this.Test_RegistrationFunc(telimena, () => telimena.Initialize().GetAwaiter().GetResult(), false);
        }

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

                StringAssert.Contains("An error occurred while posting to [api/v1/telemetry/initialize]", ex.InnerExceptions[0].Message);
                TelemetryInitializeRequest jObj = ex.RequestObjects[0].Value as TelemetryInitializeRequest;
                ((Telimena)telimena).Properties.StaticProgramInfo.ThrowIfPublicPropertiesNotEqual(jObj.ProgramInfo, true);
            }
        }


        [Test]
        public void Test_RegisterRequestCreation_EmptyKey()
        {
            Telimena telimena = TelimenaFactory.Construct(new TelimenaStartupInfo(Guid.Empty)) as Telimena;
            telimena.Properties.SuppressAllErrors = false;
            Helpers.SetupMockHttpClient(telimena, Helpers.GetMockClient());

            try
            {
                telimena.Initialize().GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.InnerException.Message.Contains("Tracking key is an empty guid."));
            }
        }
    }
}