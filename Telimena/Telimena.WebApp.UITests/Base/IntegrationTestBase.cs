using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
using AutomaticTestsClient;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NUnit.Framework;
using PackageTriggerUpdaterTestApp;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Models.Account;
using Telimena.WebApp.UITests.Base.TestAppInteraction;
using TelimenaClient;
using TelimenaClient.Model;
using TestStack.White;

namespace Telimena.WebApp.UITests.Base
{
    [TestFixture]
    public abstract class IntegrationTestBase : TestBase
    {
        public readonly string AdminName = GetSetting<string>(ConfigKeys.AdminName);
        public readonly string UserName = GetSetting(ConfigKeys.UserName);
        public readonly string AdminPassword = GetSetting(ConfigKeys.AdminPassword);
        public readonly string UserPassword = GetSetting(ConfigKeys.UserPassword);

        
        protected List<string> errors = new List<string>();
        protected List<string> outputs = new List<string>();
        private readonly bool isLocalTestSetting = GetSetting<bool>(ConfigKeys.IsLocalTest);
        public const string AutomaticTestsClientTelemetryKey = "efacd375-d746-48de-9882-b7ca4426d1e2";
        public const string PackageUpdaterClientTelemetryKey = "43808405-afca-4abb-a92a-519489d62290";
        protected ITestEngine TestEngine { get; set; }

        private string BaseUrl => this.TestEngine.BaseUrl;

        private HttpClient HttpClient { get; } = new HttpClient();

        protected Exception CleanupAndRethrow(Exception ex, [CallerMemberName] string caller = "")
        {
            string msg =
                $"Error when executing test: {caller}" + 
                $"\r\nOutputs:\r\n{String.Join("\r\n", this.outputs)}" + 
                $"\r\n\r\nErrors:\r\n{string.Join("\r\n", this.errors)}\r\n. See inner exception.";
            TestAppProvider.KillTestApps();
            return new InvalidOperationException(msg, ex);
        }

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

        protected async Task<TelemetryQueryResponse> CheckTelemetry(TelemetryQueryRequest request)
        {
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync(this.BaseUrl.TrimEnd('/') + "/api/v1/telemetry/execute-query", request).ConfigureAwait(false);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<TelemetryQueryResponse>(content);
        }

        protected FileInfo LaunchTestsAppNewInstance(out Process process, Actions action, string appName, string testSubfolderName, ProgramInfo pi = null
            , string viewName = null, bool waitForExit = true)
        {
            var appFile = TestAppProvider.ExtractApp(appName, testSubfolderName);

            process = this.LaunchTestsApp(appFile, action, pi, waitForExit, viewName);
            return appFile;
        }

        protected FileInfo LaunchTestsAppNewInstanceAndGetResult<T>(out Process process, out T result, Actions action, string appName, string testSubfolderName, ProgramInfo pi = null
            , string viewName = null, bool waitForExit = true) where T : class
        {
            var appFile = TestAppProvider.ExtractApp(appName, testSubfolderName);


            this.outputs.Clear();
            this.errors.Clear();

            process = this.LaunchTestsApp(appFile, action, pi, waitForExit, viewName);
            
            result = this.ParseOutput<T>();
            this.outputs.Clear();
            this.errors.Clear();


            return appFile;
        }

        protected Process LaunchTestsApp(FileInfo appFile, Actions action, ProgramInfo pi = null, bool waitForExit = true, string viewName = null)
        {
            Arguments args = new Arguments { ApiUrl = this.BaseUrl, Action = action };
            args.DebugMode = true;
            args.ProgramInfo = pi;
            args.ViewName = viewName;
            args.TelemetryKey = Guid.Parse(AutomaticTestsClientTelemetryKey);


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

        protected Process LaunchPackageUpdaterTestsAppWithArgs(out FileInfo appFile, string appName, string testSubfolderName, bool waitForExit)
        {
             appFile = TestAppProvider.ExtractApp(appName, testSubfolderName);
            PackageUpdateTesterArguments args = new PackageUpdateTesterArguments { ApiUrl = this.BaseUrl };
            args.TelemetryKey = Guid.Parse(PackageUpdaterClientTelemetryKey);

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

        protected T LaunchPackageUpdaterTestsAppAndGetResult<T>(string appName, string testSubfolderName, bool waitForExit) where T : class
        {
            var appFile = TestAppProvider.ExtractApp(appName, testSubfolderName);
            PackageUpdateTesterArguments args = new PackageUpdateTesterArguments { ApiUrl = this.BaseUrl };
            args.TelemetryKey = Guid.Parse(PackageUpdaterClientTelemetryKey);


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

            T result = this.ParseOutput<T>();
            this.outputs.Clear();
            this.errors.Clear();

            return result;
        }

        protected Process LaunchPackageUpdaterTestsAppNoArgs(string appName, string testSubfolderName, bool waitForExit)
        {
            this.outputs.Clear();
            this.errors.Clear();

            var appFile = TestAppProvider.ExtractApp(appName, testSubfolderName);

            Process process = new Process();
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += (sender, args) => this.outputs.Add(args.Data);
            process.ErrorDataReceived += (sender, args) => this.errors.Add(args.Data);
            process.StartInfo = new ProcessStartInfo
            {
                FileName = appFile.FullName,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            Log($"Started process with no args: {appFile.FullName}");
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
            this.outputs.Clear();
            this.errors.Clear();

            appFile = this.LaunchTestsAppNewInstance(out _, action, appName, testSubfolderName, pi, viewName, waitForExit);

            T result = this.ParseOutput<T>();
            this.outputs.Clear();
            this.errors.Clear();

            return result;
        }

        protected T LaunchTestsAppAndGetResult<T>(FileInfo app, Actions action, ProgramInfo pi = null, string viewName = null, bool waitForExit = true) where T : class
        {
            this.outputs.Clear();
            this.errors.Clear();

            this.LaunchTestsApp(app, action, pi, waitForExit, viewName);

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
                        T obj = JsonConvert.DeserializeObject<T>(output, new JsonSerializerSettings(){TypeNameHandling = TypeNameHandling.Auto});
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