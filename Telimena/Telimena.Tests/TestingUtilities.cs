using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;

namespace Telimena.Tests
{
  public static  class TestingUtilities
    {
        public static Mock<IAssemblyVersionReader> GetMockVersionReader()
        {
            Mock<IAssemblyVersionReader> reader = new Mock<IAssemblyVersionReader>();
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
