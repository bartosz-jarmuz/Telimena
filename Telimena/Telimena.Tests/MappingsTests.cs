using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Telimena.WebApp;
using Telimena.WebApp.Core.Models;

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

    [TestFixture]
    public class EntitiesTests
    {
        [Test]
        public void ValidateNtities()
        {
            var entities = typeof(ClientAppUser).Assembly.GetTypes().Where(x => x.IsClass && x.Namespace == typeof(ClientAppUser).Namespace).ToList();
            var errors = new List<string>();
            foreach (Type entity in entities)
            {
                
            }

            if (errors.Any())
            {
                Assert.Fail(String.Join("\r\n", errors));
            }
        }

    }
}