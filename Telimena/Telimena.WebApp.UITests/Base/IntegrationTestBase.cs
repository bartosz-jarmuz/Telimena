using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AutomaticTestsClient;
using Newtonsoft.Json;
using NUnit.Framework;
using Telimena.WebApp.UITests.Base.TestAppInteraction;
using TelimenaClient;
using TestStack.White;

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

        protected FileInfo LaunchTestsAppNewInstance(out Process process, Actions action, string appName, string testSubfolderName, ProgramInfo pi = null
            , string viewName = null, bool waitForExit = true)
        {
            var appFile = TestAppProvider.ExtractApp(appName, testSubfolderName);

            process = this.LaunchTestsApp(appFile, action, pi, viewName, waitForExit);
            return appFile;
        }

        protected Process LaunchTestsApp(FileInfo appFile, Actions action, ProgramInfo pi = null, string viewName = null, bool waitForExit = true)
        {
            Arguments args = new Arguments { ApiUrl = this.BaseUrl, Action = action };
            args.ProgramInfo = pi;
            args.ViewName = viewName;


            Process process = ProcessCreator.Create(appFile, args, this.outputs, this.errors);
            Log($"Started process: {appFile.FullName}");
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            if (waitForExit)
            {
                process.WaitForExit();
                Log($"Finished process: {appFile.FullName}");
            }

            return process;
        }

        protected T LaunchTestsAppAndGetResult<T>(out FileInfo appFile, Actions action, string appName, string testSubfolderName, ProgramInfo pi = null
            , string viewName = null, bool waitForExit = true) where T : class
        {
            appFile = this.LaunchTestsAppNewInstance(out _, action, appName, testSubfolderName, pi, viewName, waitForExit);

            T result = this.ParseOutput<T>();
            this.outputs.Clear();
            this.errors.Clear();

            return result;
        }

        protected T LaunchTestsAppAndGetResult<T>(FileInfo app, Actions action, ProgramInfo pi = null, string viewName = null, bool waitForExit = true) where T : class
        {
            this.LaunchTestsApp(app, action, pi, viewName, waitForExit);

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
                    Log(output);
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