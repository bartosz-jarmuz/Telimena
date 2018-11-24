using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using NUnit.Framework;
using Telimena.WebApp.Core.Models;

namespace Telimena.Tests
{
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