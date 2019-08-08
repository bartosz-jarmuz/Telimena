using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
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
    public abstract class IntegrationTestBase 
    {
        public readonly string AdminName = GetSetting<string>(ConfigKeys.AdminName);
        public readonly string User1Name = GetSetting(ConfigKeys.User1Name);
        public readonly string User2Name = GetSetting(ConfigKeys.User2Name);
        public readonly string User3Name = GetSetting(ConfigKeys.User3Name);
        public readonly string AdminPassword = GetSetting(ConfigKeys.AdminPassword);
        public readonly string User1Password = GetSetting(ConfigKeys.User1Password);
        public readonly string User2Password = GetSetting(ConfigKeys.User2Password);
        public readonly string User3Password = GetSetting(ConfigKeys.User3Password);
        
        protected List<string> errors = new List<string>();
        protected List<string> outputs = new List<string>();
        protected readonly bool isLocalTestSetting = GetSetting<bool>(ConfigKeys.IsLocalTest);

        protected string TestRunFolderName;

        [OneTimeTearDown]
        public void TestCleanup()
        {
            this.TestEngine.BaseCleanup();
        }

        [OneTimeSetUp]
        public void TestInitialize()
        {
            this.TestRunFolderName = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") +"_"+ this.GetType().Name;
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
                var msi = TestAppProvider.GetFile(msiName);
                return this.GetCodeFromMsi(msi);
        }

        public Guid GetCodeFromMsi(FileInfo msi)
        {
            try

            {
                string productCode = "xxx";
                using (var db = new Database(msi.FullName, DatabaseOpenMode.ReadOnly))
                {
                    productCode = (string)db.ExecuteScalar("SELECT `Value` FROM " +
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

        public void InstallMsi(FileInfo msi, FileInfo expectedProgramPath)
        {
            var productCode = this.GetCodeFromMsi(msi);
            this.Log($"Installing product {productCode}. Installer path: {msi.FullName}.");

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
                    this.UninstallMsi(productCode, null);
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
                Installer.SetExternalUI(
                    delegate(InstallMessage type, string message, MessageButtons buttons, MessageIcon icon
                        , MessageDefaultButton button)
                    {
                        return MessageResult.OK;
                    }, InstallLogModes.None);
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
            catch (ArgumentException e)
            {
                if (e.Message.Contains("This action is only valid for products that are currently installed."))
                {
                    this.Log($"Product {productCode} cannot be uninstalled because it is not installed at the moment.");
                }
            }
        }

        protected static string GetSetting(string key)
        {
            if (NUnit.Framework.TestContext.Parameters.Count == 0)
            {
                return TryGetSettingFromXml(key);
            }
            var x = NUnit.Framework.TestContext.Parameters[key];
            return x;
        }

        private static string TryGetSettingFromXml(string key)
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            var file = dir.GetFiles("*.runsettings", SearchOption.AllDirectories).FirstOrDefault();
            if (file != null)
            {
                XDocument xDoc = XDocument.Load(file.FullName);
                var ele = xDoc.Root.Element("TestRunParameters").Elements().FirstOrDefault(x => x.Attribute("name")?.Value == key);
                return ele?.Attribute("value")?.Value;
            }

            return null;
        }


        protected static T GetSetting<T>(string key)
        {
            string val = GetSetting(key);
            if (val == null)
            {
                throw new ArgumentException($"Missing setting: {key}");
            }
            return (T)Convert.ChangeType(val, typeof(T));
        }


        protected void Log(string info)
        {
            //Trace.TraceInformation("trace - UiTestsLogger:" + info);
            //Trace.TraceError("error - UiTestsLogger:" + info);
            //Logger.LogMessage("Logger - UiTestsLogger:" + info);
            //TestContext.Out.WriteLine("Ctx -  UiTestsLogger:" + info);
            Console.WriteLine("UiTestsLogger:" + info);
        }

        private HttpClient HttpClient { get; } = new HttpClient();


        protected async Task<TelemetryQueryResponse> CheckTelemetry(TelemetryQueryRequest request)
        {
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync(this.BaseUrl.TrimEnd('/') + "/api/v1/telemetry/execute-query", request).ConfigureAwait(false);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<TelemetryQueryResponse>(content);
        }



        public async Task SendBasicTelemetry(Guid guid, BasicTelemetryItem telemetry)
        {
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync($"{this.BaseUrl.TrimEnd('/')}/api/v1/telemetry/{guid.ToString()}/basic", telemetry).ConfigureAwait(false);
            var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Console.WriteLine(text);
            response.EnsureSuccessStatusCode();
        }


        protected ITestEngine TestEngine { get; set; }

        protected string BaseUrl => this.TestEngine.BaseUrl;


        protected T ParseOutput<T>() where T : class
        {
            foreach (string output in this.outputs)
            {
                if (!string.IsNullOrWhiteSpace(output))
                {
                    this.Log(output);
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

        protected FileInfo LaunchTestsAppNewInstance(out Process process, Actions action, Guid telemetryKey, string appName, string testSubfolderName, ProgramInfo pi = null
           , string viewName = null, bool waitForExit = true)
        {
            var appFile = TestAppProvider.ExtractApp(appName, testSubfolderName, this.Log);

            process = this.LaunchTestsApp(appFile, action, telemetryKey, pi, waitForExit, viewName);
            return appFile;
        }

        protected FileInfo LaunchTestsAppNewInstanceAndGetResult<T>(out Process process, out T result, Actions action, Guid telemetryKey, string appName, string testSubfolderName, ProgramInfo pi = null
            , string viewName = null, bool waitForExit = true) where T : class
        {
            var appFile = TestAppProvider.ExtractApp(appName, testSubfolderName, this.Log);


            this.outputs.Clear();
            this.errors.Clear();

            process = this.LaunchTestsApp(appFile, action, telemetryKey, pi, waitForExit, viewName);

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
            this.Log($"Started process: {appFile.FullName}");
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            if (waitForExit)
            {
                process.WaitForExit();
                this.Log($"Finished process: {appFile.FullName}");
            }

            return process;
        }

        protected Process LaunchPackageUpdaterTestsAppWithArgs(out FileInfo appFile, string appName, string testSubfolderName, bool waitForExit)
        {
            appFile = TestAppProvider.ExtractApp(appName, testSubfolderName, this.Log);
            Arguments args = new Arguments { ApiUrl = this.BaseUrl };

            args.TelemetryKey = Apps.Keys.PackageUpdaterClient;

            Process process = ProcessCreator.Create(appFile, args, this.outputs, this.errors);
            this.Log($"Started process: {appFile.FullName}");
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            if (waitForExit)
            {
                process.WaitForExit();
                this.Log($"Finished process: {appFile.FullName}");
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
            else
            {
                this.Log($"Failed to deserialize the outputs as {typeof(T).Name}");
            }
            return result;
        }
    }
}