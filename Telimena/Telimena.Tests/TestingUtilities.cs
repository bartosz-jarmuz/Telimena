using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;

namespace Telimena.Tests
{
  public static  class TestingUtilities
    {

        public static void SetReuqest(ApiController controller, HttpMethod method, Dictionary<string, object> properties, string url = "http://mstest/things/1")
        {
            var httpConfig = new HttpConfiguration();
            controller.Configuration = httpConfig;

            // Fake the request.
            //
            var httpRequest = new HttpRequestMessage(method, url);
            foreach (KeyValuePair<string, object> keyValuePair in properties)
            {
                httpRequest.Properties.Add(keyValuePair.Key, keyValuePair.Value);
            }
            httpRequest.Properties[HttpPropertyKeys.HttpConfigurationKey] = httpConfig;

            controller.Request = httpRequest;
        }

        public static Mock<IAssemblyStreamVersionReader> GetMockVersionReader()
        {
            Mock<IAssemblyStreamVersionReader> reader = new Mock<IAssemblyStreamVersionReader>();
            reader.Setup(x => x.GetFileVersion(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<bool>())).Returns((Stream stream, string assName, bool singleFile) =>
            {
                var str = TestingUtilities.ExtractString(stream);
                Assert.IsTrue(stream.CanRead);
                return Task.FromResult(str);
            });
            return reader;
        }

        public static string ExtractString(Stream stream, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            using (var sr = new StreamReader(stream, encoding, false, 1024, true))
            {
                return sr.ReadToEnd();
            }
        }

        public static IFileSaver MockSaver()
        {
            return new Mock<IFileSaver>().Object;
        }

        public static Stream GenerateStream(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
