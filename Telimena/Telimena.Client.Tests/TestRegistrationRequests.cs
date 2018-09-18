// -----------------------------------------------------------------------
//  <copyright file="TestRegistrationRequests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using DotNetLittleHelpers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Telimena.Client.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestRegistrationRequests
    {
        public void Test_RegistrationFunction(Telimena telimena, Func<RegistrationResponse> function, bool skipFlagExpectedValue)
        {
            try
            {
                RegistrationResponse result = function();
                Assert.Fail("Exception expected");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;
                Assert.AreEqual(2, ex.InnerExceptions.Count);
                Assert.AreEqual("api/Statistics/RegisterClient", ex.InnerExceptions[0].Message);
                RegistrationRequest jObj = JsonConvert.DeserializeObject<RegistrationRequest>(ex.InnerExceptions[1].Message);
                Assert.AreEqual(skipFlagExpectedValue, jObj.SkipUsageIncrementation);
                telimena.ProgramInfo.ThrowIfPublicPropertiesNotEqual(jObj.ProgramInfo, true);
            }
        }

        [Test]
        public void Test_InitializeRequestCreation()
        {
            Telimena telimena = new Telimena();
            telimena.SuppressAllErrors = false;
            telimena.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");
            Helpers.SetupMockHttpClient(telimena, Helpers.GetMockClient());
            this.Test_RegistrationFunction(telimena, () => telimena.Initialize().GetAwaiter().GetResult(), false);
        }

        [Test]
        public void Test_RegisterRequestCreation()
        {
            Telimena telimena = new Telimena();
            telimena.SuppressAllErrors = false;
            telimena.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");
            Helpers.SetupMockHttpClient(telimena, Helpers.GetMockClient());
            this.Test_RegistrationFunction(telimena, () => telimena.RegisterClient().GetAwaiter().GetResult(), false);
            this.Test_RegistrationFunction(telimena, () => telimena.RegisterClient(true).GetAwaiter().GetResult(), true);
        }
    }
}