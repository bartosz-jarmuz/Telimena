// -----------------------------------------------------------------------
//  <copyright file="ClientTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Telimena.Updater;
using Telimena.WebApp.Core.Models;

namespace TelimenaClient.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class UtilitiesTests
    {

        private string GetIt()
        {
            return "aaa";
        }


        [Test]
        public async Task Sandbox()
        {
            var list = new List<string>();
            for (int i = 0; i < 1000000; i++)
            {
                list.Add(this.GetIt());
            }

        }

       
    }
}