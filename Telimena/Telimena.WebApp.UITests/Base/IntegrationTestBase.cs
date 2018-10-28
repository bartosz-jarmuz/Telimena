using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AutomaticTestsClient;
using Newtonsoft.Json;
using NUnit.Framework;
using Telimena.WebApp.UITests.Base.TestAppInteraction;
using TelimenaClient;

namespace Telimena.WebApp.UITests.Base
{
    [TestFixture]
    public abstract class IntegrationTestBase : TestBase
    {
        protected List<string> errors = new List<string>();
        protected List<string> outputs = new List<string>();
        private readonly bool isLocalTestSetting = GetSetting<bool>(ConfigKeys.IsLocalTest);
        protected ITestEngine TestEngine { get; set; }

        protected string BaseUrl => this.TestEngine.BaseUrl;

        [SetUp]
        public void ResetLists()
        {
            this.errors = new List<string>();
            this.outputs = new List<string>();
        }

        [OneTimeTearDown]
        public void TestCleanup()
        {
            this.TestEngine.BaseCleanup();
        }

        [OneTimeSetUp]
        public void TestInitialize()
        {
            if (this.isLocalTestSetting)
            {
                this.TestEngine = new LocalHostTestEngine();
            }
            else
            {
                this.TestEngine = new DeployedTestEngine(GetSetting<string>(ConfigKeys.PortalUrl));
            }

            this.TestEngine.BaseInitialize();
        }

        protected void LaunchTestsApp(Actions action, string appName, string testSubfolderName, ProgramInfo pi = null, string functionName = null
            , bool waitForExit = true)
        {
            FileInfo exe = TestAppProvider.ExtractApp(appName, testSubfolderName);

            Arguments args = new Arguments {ApiUrl = this.BaseUrl, Action = action};
            args.ProgramInfo = pi;
            args.FunctionName = functionName;


            Process process = ProcessCreator.Create(exe, args, this.outputs, this.errors);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            if (waitForExit)
            {
                process.WaitForExit();
            }
        }

        protected T LaunchTestsAppAndGetResult<T>(Actions action, string appName, string testSubfolderName, ProgramInfo pi = null, string functionName = null
            , bool waitForExit = true) where T : class
        {
            this.LaunchTestsApp(action, appName, testSubfolderName, pi, functionName, waitForExit);

            T result = this.ParseOutput<T>();
            this.outputs.Clear();
            this.errors.Clear();
            return result;
        }

        protected T ParseOutput<T>() where T : class
        {
            foreach (string output in this.outputs)
            {
                if (!string.IsNullOrWhiteSpace(output))
                {
                    try
                    {
                        T obj = JsonConvert.DeserializeObject<T>(output);
                        if (obj != null)
                        {
                            return obj;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return null;
        }
    }
}