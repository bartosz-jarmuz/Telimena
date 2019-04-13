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
using Microsoft.Deployment.WindowsInstaller;
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
        public static bool ShowBrowser
        {
            get
            {
                try
                {
#if DEBUG
                    return true;
#endif
                    return GetSetting<bool>(ConfigKeys.ShowBrowser);
                }
                catch
                {
                    return false;
                }
            }
        }

        public readonly string AdminName = GetSetting<string>(ConfigKeys.AdminName);
        public readonly string UserName = GetSetting(ConfigKeys.UserName);
        public readonly string AdminPassword = GetSetting(ConfigKeys.AdminPassword);
        public readonly string UserPassword = GetSetting(ConfigKeys.UserPassword);

        
        protected List<string> errors = new List<string>();
        protected List<string> outputs = new List<string>();
        private readonly bool isLocalTestSetting = GetSetting<bool>(ConfigKeys.IsLocalTest);

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

        public Guid GetCodeFromMsi(string  msiName)
        {
            try

            {

                var msi = TestAppProvider.GetFile(msiName);
                string productCode = "xxx";
                using (var db = new Database(msi.FullName, DatabaseOpenMode.ReadOnly))
                {
                    productCode = (string) db.ExecuteScalar("SELECT `Value` FROM " +
                                                            "`Property` WHERE `Property` = 'ProductCode'");
                }

                return Guid.Parse(productCode);
            }
            catch (ArgumentException e) when (e.Message.Contains(
                "This action is only valid for products that are currently installed."))
            {
                //ok
            }

            return Guid.Empty;
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

        public void InstallMsi(FileInfo msi, FileInfo expectedProgramPath)
        {
            Process.Start(msi.FullName);
            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < 1000 * 60)
            {
                if (File.Exists(expectedProgramPath.FullName))
                {
                    break;
                }
            }
        }

        public void UninstallPackages(params Guid[] productCodes)
        {
            try
            {
                foreach (Guid productCode in productCodes)
                {
                    UninstallMsi(productCode, null);
                }
            }
            catch (ArgumentException e) when (e.Message.Contains("This action is only valid for products that are currently installed."))
            {
                //ok
            }
        }

        public void UninstallMsi(Guid productCode, FileInfo expectedProgramPath )
        {
            try
            {
                Installer.ConfigureProduct("{" + productCode + "}", 0, InstallState.Absent, "");
                this.Log($"Uninstalled package {productCode}.");

                if (expectedProgramPath != null)
                {
                    var sw = Stopwatch.StartNew();
                    while (sw.ElapsedMilliseconds < 1000 * 60)
                    {
                        if (!File.Exists(expectedProgramPath.FullName))
                        {

                            break;
                        }
                    }
                }
             
            }
            catch (ArgumentException e) when (e.Message.Contains("This action is only valid for products that are currently installed."))
            {
                //ok
            }
        }

        protected async Task<TelemetryQueryResponse> CheckTelemetry(TelemetryQueryRequest request)
        {
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync(this.BaseUrl.TrimEnd('/') + "/api/v1/telemetry/execute-query", request).ConfigureAwait(false);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<TelemetryQueryResponse>(content);
        }

        protected FileInfo LaunchTestsAppNewInstance(out Process process, Actions action, Guid telemetryKey, string appName, string testSubfolderName, ProgramInfo pi = null
            , string viewName = null, bool waitForExit = true)
        {
            var appFile = TestAppProvider.ExtractApp(appName, testSubfolderName, this.Log);

            process = this.LaunchTestsApp(appFile, action,telemetryKey, pi, waitForExit, viewName);
            return appFile;
        }

        protected FileInfo LaunchTestsAppNewInstanceAndGetResult<T>(out Process process, out T result, Actions action, Guid telemetryKey, string appName, string testSubfolderName, ProgramInfo pi = null
            , string viewName = null, bool waitForExit = true) where T : class
        {
            var appFile = TestAppProvider.ExtractApp(appName, testSubfolderName, this.Log);


            this.outputs.Clear();
            this.errors.Clear();

            process = this.LaunchTestsApp(appFile, action,telemetryKey, pi, waitForExit, viewName);
            
            result = this.ParseOutput<T>();
            this.outputs.Clear();
            this.errors.Clear();


            return appFile;
        }

        protected Process LaunchTestsApp(FileInfo appFile, Actions action, Guid telemetryKey, ProgramInfo pi = null, bool waitForExit = true, string viewName = null)
        {
            Arguments args = new Arguments { ApiUrl = this.BaseUrl, Action = action };
            args.DebugMode = true;
            args.ProgramInfo = pi;
            args.ViewName = viewName;
            args.TelemetryKey = telemetryKey;


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
             appFile = TestAppProvider.ExtractApp(appName, testSubfolderName, this.Log);
            PackageUpdateTesterArguments args = new PackageUpdateTesterArguments { ApiUrl = this.BaseUrl };

            args.TelemetryKey = Apps.Keys.PackageUpdaterClient;

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


        protected T LaunchTestsAppNewInstanceAndGetResult<T>(out FileInfo appFile, Actions action, Guid telemetryKey, string appName, string testSubfolderName, ProgramInfo pi = null
            , string viewName = null, bool waitForExit = true) where T : class
        {
            this.outputs.Clear();
            this.errors.Clear();

            appFile = this.LaunchTestsAppNewInstance(out _, action, telemetryKey, appName, testSubfolderName, pi, viewName, waitForExit);

            T result = this.ParseOutput<T>();
            this.outputs.Clear();
            this.errors.Clear();

            return result;
        }

        protected T LaunchTestsAppAndGetResult<T>(FileInfo app, Actions action, Guid telemetryKey, ProgramInfo pi = null, string viewName = null, bool waitForExit = true) where T : class
        {
            this.outputs.Clear();
            this.errors.Clear();

            this.LaunchTestsApp(app, action, telemetryKey, pi, waitForExit, viewName);

            T result = this.ParseOutput<T>();
            if (result != null)
            {
                this.outputs.Clear();
                this.errors.Clear();
            }

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