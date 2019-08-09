using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Deployment.WindowsInstaller;
using Newtonsoft.Json;
using NUnit.Framework;
using SharedLogic;
using Telimena.TestUtilities.Base.TestAppInteraction;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Messages;
using TelimenaClient.Model;

namespace Telimena.TestUtilities.Base
{
    [TestFixture]
    public abstract partial class IntegrationTestBase 
    {
        public static string TestRunTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
        private HttpClient HttpClient { get; } = new HttpClient();
        protected ITestEngine TestEngine { get; set; }

        protected string BaseUrl => this.TestEngine.BaseUrl;

        [OneTimeTearDown]
        public void TestCleanup()
        {
            this.TestEngine.BaseCleanup();
        }

        [OneTimeSetUp]
        public void TestInitialize()
        {
            if (this.IsLocalTestSetting)
            {
                this.TestEngine = new LocalHostTestEngine();
            }
            else
            {
                this.TestEngine = new DeployedTestEngine(ConfigHelper.GetSetting<string>(ConfigKeys.PortalUrl));
            }

            this.TestEngine.BaseInitialize();
        }

        [SetUp]
        public void ResetLists()
        {
            this.errors = new List<string>();
            this.outputs = new List<string>();
        }

    }
}