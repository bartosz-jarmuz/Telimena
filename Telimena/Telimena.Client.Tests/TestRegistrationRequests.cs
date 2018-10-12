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
        public void Test_RegistrationFunction(TelimenaClient.Telimena telimena, Func<RegistrationResponse> function, bool skipFlagExpectedValue)
        {
            try
            {
                RegistrationResponse result = function(); 
                Assert.Fail("Exception expected");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreEqual("An error occured while posting to [api/Statistics/RegisterClient]", ex.InnerExceptions[0].Message);
                RegistrationRequest jObj = ex.RequestObjects[0].Value as RegistrationRequest;
                Assert.AreEqual(skipFlagExpectedValue, jObj.SkipUsageIncrementation);
                telimena.StaticProgramInfo.ThrowIfPublicPropertiesNotEqual(jObj.ProgramInfo, true);
            }
        }

        [Test]
        public void Test_InitializeRequestCreation()
        {
            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena();
            telimena.SuppressAllErrors = false;
            telimena.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");
            Helpers.SetupMockHttpClient(telimena, Helpers.GetMockClient());
            this.Test_RegistrationFunction(telimena, () => telimena.InitializeAsync().GetAwaiter().GetResult(), false);
            this.Test_RegistrationFunction(telimena, () => telimena.InitializeBlocking(), false);
        }

        [Test]
        public void Test_RegisterRequestCreation()
        {
            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena();
            telimena.SuppressAllErrors = false;
            telimena.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");
            Helpers.SetupMockHttpClient(telimena, Helpers.GetMockClient());
            this.Test_RegistrationFunction(telimena, () => telimena.RegisterClient().GetAwaiter().GetResult(), false);
            this.Test_RegistrationFunction(telimena, () => telimena.RegisterClient(true).GetAwaiter().GetResult(), true);
        }
    }
}