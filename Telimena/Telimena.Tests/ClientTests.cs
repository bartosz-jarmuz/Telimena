using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.Tests
{
    using Client;
    using NUnit.Framework;

    [TestFixture]
    public class ClientTests
    {
        [Test]
        public void Initialization()
        {
            Telimena telimena = new Telimena();
        }
    }
}
