// -----------------------------------------------------------------------
//  <copyright file="ClientTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Telimena.Tests
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Client;
    using Client.Model;
    using DotNetLittleHelpers;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;
    #endregion

    [TestFixture]
    public class ClientTests
    {


        [Test]
        public void Test_CheckForUpdates()
        {
            Telimena sut = new Telimena()
            {
                SuppressAllErrors = false
            };
            Assert.AreEqual("Telimena.Client", sut.ProgramInfo.PrimaryAssembly.Name);

            sut.LoadHelperAssembliesByName("Telimena.Tests.dll", "Moq.dll");

            var latestVersionResponse = new LatestVersionResponse()
            {
                PrimaryAssemblyVersion = new VersionInfo()
                {
                    AssemblyId = 1,
                    AssemblyName = "Telimena.Client",
                    LatestVersion = "3.1.0.0",
                    LatestVersionId = 3
                },
                HelperAssemblyVersions = new List<VersionInfo>()
                {
                    new VersionInfo()
                    {
                        AssemblyName = "Telimena.Tests",
                        LatestVersion = "3.1.0.1"
                    }
                }
            };
            this.SetupMockHttpClient(sut, this.GetMockClientForCheckForUpdates(latestVersionResponse));

            UpdateCheckResult response = sut.CheckForUpdates().GetAwaiter().GetResult();
            Assert.IsTrue(response.IsUpdateAvailable);
            Assert.AreEqual("3.1.0.0", response.PrimaryAssemblyUpdateInfo.LatestVersionInfo.LatestVersion);
            Assert.AreEqual("3.1.0.1", response.HelperAssembliesToUpdate.Single().LatestVersionInfo.LatestVersion);

        }

        [Test]
        public void Test_InitializeRequestCreation()
        {
            Telimena telimena = new Telimena();
            telimena.SuppressAllErrors = false;
            telimena.LoadHelperAssembliesByName("Telimena.Tests.dll", "Moq.dll");
            this.SetupMockHttpClient(telimena, this.GetMockClient());
            this.Test_RegistrationFunction(telimena, () => telimena.Initialize().GetAwaiter().GetResult(), false);
        }

        [Test]
        public void Test_RegisterRequestCreation()
        {
            Telimena telimena = new Telimena();
            telimena.SuppressAllErrors = false;
            telimena.LoadHelperAssembliesByName("Telimena.Tests.dll", "Moq.dll");
            this.SetupMockHttpClient(telimena, this.GetMockClient());
            this.Test_RegistrationFunction(telimena, () => telimena.RegisterClient().GetAwaiter().GetResult(), false);
            this.Test_RegistrationFunction(telimena, () => telimena.RegisterClient(true).GetAwaiter().GetResult(), true);
        }

        public void Test_RegistrationFunction(Telimena telimena, Func<RegistrationResponse> function, bool skipFlagExpectedValue)
        {
            try
            {
                RegistrationResponse result = function();
                Assert.Fail("Exception expected");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(2, ex.InnerExceptions.Count);
                Assert.AreEqual("api/Statistics/RegisterClient", ex.InnerExceptions[0].Message);
                var jObj = JsonConvert.DeserializeObject<RegistrationRequest>(ex.InnerExceptions[1].Message);
                Assert.AreEqual(skipFlagExpectedValue, jObj.SkipUsageIncrementation);
                telimena.ProgramInfo.ThrowIfPublicPropertiesNotEqual(jObj.ProgramInfo, recursiveValidation: true);
                return;
            }
        }


        [Test]
        public void Test_StaticClient_WithProgramInfo()
        {
            Mock<ITelimenaHttpClient> client = this.GetMockClient();
            var pi = new ProgramInfo()
            {
                Name = "An App!",
                PrimaryAssembly = new AssemblyInfo(this.GetType().Assembly)
            };
            try
            {
                var result = Telimena.SendUsageReport(client.Object, pi, suppressAllErrors: false).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(2, ex.InnerExceptions.Count);
                Assert.AreEqual("api/Statistics/RegisterClient", ex.InnerExceptions[0].Message);
                var jObj = JsonConvert.DeserializeObject<RegistrationRequest>(ex.InnerExceptions[1].Message);
                Assert.AreEqual(true, jObj.SkipUsageIncrementation);
                pi.ThrowIfPublicPropertiesNotEqual(jObj.ProgramInfo, recursiveValidation:true);
            }
        }

        [Test]
        public void Test_StaticClient_RegisterRequest()
        {
            Mock<ITelimenaHttpClient> client = this.GetMockClient();
            try
            {
                var result = Telimena.SendUsageReport(client.Object, suppressAllErrors: false).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(2, ex.InnerExceptions.Count);
                Assert.AreEqual("api/Statistics/RegisterClient", ex.InnerExceptions[0].Message);
                var jObj = JsonConvert.DeserializeObject<RegistrationRequest>(ex.InnerExceptions[1].Message);
                Assert.AreEqual(true, jObj.SkipUsageIncrementation);
                Assert.AreEqual("Telimena.Client", jObj.ProgramInfo.Name);
                Assert.AreEqual("Telimena.Client", jObj.ProgramInfo.PrimaryAssembly.Name);
            }

            client = this.GetMockClientForStaticClient_FirstRequestPass();
            try
            {
                var result = Telimena.SendUsageReport(client.Object, suppressAllErrors: false).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(2, ex.InnerExceptions.Count);
                Assert.AreEqual("api/Statistics/UpdateProgramStatistics", ex.InnerExceptions[0].Message);
                var jObj = JsonConvert.DeserializeObject<StatisticsUpdateRequest>(ex.InnerExceptions[1].Message);
                Assert.AreEqual(1, jObj.ProgramId);
                Assert.AreEqual(2, jObj.UserId);
                Assert.AreEqual("Test_StaticClient_RegisterRequest", jObj.FunctionName);
                Assert.AreEqual("1.0.0.0", jObj.Version);
            }
        }

        [Test]
        public void TestInitialize_AssemblyParameter()
        {
            Telimena telimena = new Telimena(this.GetType().Assembly);
            Assert.AreEqual("Telimena.Tests", telimena.ProgramInfo.Name);
            Assert.AreEqual("Telimena.Tests", telimena.ProgramInfo.PrimaryAssembly.Name);
            Assert.IsNotNull(telimena.ProgramInfo.PrimaryAssembly.Version);

            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
        }

        [Test]
        public void TestInitialize_LoadHelperAssemblies_ByAssembly()
        {
            Telimena telimena = new Telimena();
            telimena.LoadHelperAssemblies(this.GetType().Assembly, typeof(Capture).Assembly);
            Assert.AreEqual(2, telimena.ProgramInfo.HelperAssemblies.Count);
            Assert.AreEqual(1, telimena.ProgramInfo.HelperAssemblies.Count(x => x.Name == "Telimena.Tests"));
            Assert.AreEqual(1, telimena.ProgramInfo.HelperAssemblies.Count(x => x.Name == "Moq"));
            Assert.IsTrue(telimena.ProgramInfo.HelperAssemblies.All(x => x.Version != null && x.Name != null));
        }

        [Test]
        public void TestInitialize_LoadHelperAssemblies_ByString()
        {
            Telimena telimena = new Telimena();
            telimena.LoadHelperAssembliesByName("Telimena.Tests.dll", "Moq.dll");
            Assert.AreEqual(2, telimena.ProgramInfo.HelperAssemblies.Count);
            Assert.AreEqual(1, telimena.ProgramInfo.HelperAssemblies.Count(x => x.Name == "Telimena.Tests"));
            Assert.AreEqual(1, telimena.ProgramInfo.HelperAssemblies.Count(x => x.Name == "Moq"));
            Assert.IsTrue(telimena.ProgramInfo.HelperAssemblies.All(x => x.Version != null && x.Name != null));
        }

        [Test]
        public void TestInitialize_NoParameters()
        {
            Telimena telimena = new Telimena();
            Assert.AreEqual("Telimena.Client", telimena.ProgramInfo.Name);
            Assert.AreEqual("Telimena.Client", telimena.ProgramInfo.PrimaryAssembly.Name);
            Assert.IsNotNull(telimena.ProgramInfo.PrimaryAssembly.Version);
            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
        }

        [Test]
        public void TestInitialize_ProgramInfo()
        {
            var pi = new ProgramInfo()
            {
                Name = "An App!",
                PrimaryAssembly = new AssemblyInfo(this.GetType().Assembly)
            };
            Telimena telimena = new Telimena(pi);
            Assert.AreEqual("An App!", telimena.ProgramInfo.Name);
            Assert.AreEqual("Telimena.Tests", telimena.ProgramInfo.PrimaryAssembly.Name);
            Assert.IsNotNull(telimena.ProgramInfo.PrimaryAssembly.Version);
            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
        }

        private Mock<ITelimenaHttpClient> GetMockClient()
        {
            Mock<ITelimenaHttpClient> client = new Mock<ITelimenaHttpClient>();
            client.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).Callback((string uri, HttpContent content) =>
            {
                throw new AggregateException(
                    new AssertionException(uri),
                    new AssertionException(content.ReadAsStringAsync().GetAwaiter().GetResult()));
            });
            return client;
        }

        private Mock<ITelimenaHttpClient> GetMockClientForCheckForUpdates(object responseObj)
        {
            Mock<ITelimenaHttpClient> client = new Mock<ITelimenaHttpClient>();
            client.Setup(x => x.PostAsync("api/Statistics/RegisterClient", It.IsAny<HttpContent>())).Returns((string uri, HttpContent requestContent) =>
            {
                var response = new HttpResponseMessage();
                var registrationResponse = new RegistrationResponse()
                {
                    Count = 0,
                    ProgramId = 1,
                    UserId = 2
                };
                response.Content = new StringContent(JsonConvert.SerializeObject(registrationResponse));
                return Task.FromResult(response);
            });
            client.Setup(x => x.GetAsync(It.IsAny<string>())).Returns((string uri) =>
            {
                var response = new HttpResponseMessage();
               
                response.Content = new StringContent(JsonConvert.SerializeObject(responseObj));
                return Task.FromResult(response);
            });
            return client;
        }

        private Mock<ITelimenaHttpClient> GetMockClientForStaticClient_FirstRequestPass()
        {
            Mock<ITelimenaHttpClient> client = new Mock<ITelimenaHttpClient>();
            client.Setup(x => x.PostAsync("api/Statistics/RegisterClient", It.IsAny<HttpContent>())).Returns((string uri, HttpContent requestContent) =>
            {
                var response = new HttpResponseMessage();
                var registrationResponse = new RegistrationResponse()
                {
                    Count = 0,
                    ProgramId = 1,
                    UserId = 2
                };
                response.Content = new StringContent(JsonConvert.SerializeObject(registrationResponse));
                return Task.FromResult(response);
            });
            client.Setup(x => x.PostAsync("api/Statistics/UpdateProgramStatistics", It.IsAny<HttpContent>())).Callback((string uri, HttpContent requestContent) =>
            {
                throw new AggregateException(
                    new AssertionException(uri),
                    new AssertionException(requestContent.ReadAsStringAsync().GetAwaiter().GetResult()));
            });
            return client;
        }

        private void SetupMockHttpClient(Telimena telimena, Mock<ITelimenaHttpClient> client)
        {
            telimena.Messenger = new Messenger(telimena.Serializer, client.Object, telimena.SuppressAllErrors);
        }
    }
}