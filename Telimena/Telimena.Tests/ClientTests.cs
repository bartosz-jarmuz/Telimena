using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.Tests
{
    using System.Net.Http;
    using System.Reflection;
    using Client;
    using DotNetLittleHelpers;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class ClientTests
    {
        [Test]
        public void TestInitialize_NoParameters()
        {
            Telimena telimena = new Telimena();
            Assert.AreEqual("Telimena.Client", telimena.ProgramInfo.Name);
            Assert.AreEqual("Telimena.Client", telimena.ProgramInfo.PrimaryAssembly.Name);
            Assert.IsNotNull( telimena.ProgramInfo.PrimaryAssembly.Version);
            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
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
            telimena.LoadHelperAssemblies(this.GetType().Assembly, typeof(Moq.Capture).Assembly);
            Assert.AreEqual(2, telimena.ProgramInfo.HelperAssemblies.Count);
            Assert.AreEqual(1, telimena.ProgramInfo.HelperAssemblies.Count(x=>x.Name == "Telimena.Tests"));
            Assert.AreEqual(1, telimena.ProgramInfo.HelperAssemblies.Count(x=>x.Name == "Moq"));
            Assert.IsTrue(telimena.ProgramInfo.HelperAssemblies.All(x=>x.Version != null && x.Name != null));
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

        private void MockHttpClient(Telimena telimena)
        {
            Mock<ITelimenaHttpClient> client = new Mock<ITelimenaHttpClient>();
            client.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).Callback((string uri, HttpContent content) =>
            {
                throw new AggregateException(
                    new AssertionException(uri),
                    new AssertionException(content.ReadAsStringAsync().GetAwaiter().GetResult()));
            });
            telimena.Messenger = new Messenger(telimena.Serializer, client.Object, telimena.SuppressAllErrors);
        }

        [Test]
        public void Test_RequestCreation()
        {
            Telimena telimena = new Telimena();
            telimena.SuppressAllErrors = false;
            telimena.LoadHelperAssembliesByName("Telimena.Tests.dll", "Moq.dll");
            this.MockHttpClient(telimena);
            try
            {
                RegistrationResponse result = telimena.Initialize().GetAwaiter().GetResult();
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(2, ex.InnerExceptions.Count);
                Assert.AreEqual("api/Statistics/RegisterClient", ex.InnerExceptions[0].Message);
                var jObj = JsonConvert.DeserializeObject<RegistrationRequest>(ex.InnerExceptions[1].Message);
                telimena.ProgramInfo.ThrowIfPublicPropertiesNotEqual(jObj.ProgramInfo, recursiveValidation: true);
                return;
            }
            Assert.Fail("Exception expected");
        }

    }
}
