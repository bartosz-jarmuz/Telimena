using System;
using NUnit.Framework;
using Telimena.WebApp;

namespace Telimena.Tests
{
    [TestFixture]
    public class MappingsTests
    {
        [Test]
        public void EnsureAutoMapperConfigIsValid()
        {
            AutoMapperConfiguration.Validate();
        }
    }
}