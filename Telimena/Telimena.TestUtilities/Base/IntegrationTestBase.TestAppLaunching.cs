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
        protected FileInfo LaunchTestsAppNewInstance(out Process process, Actions action, Guid telemetryKey, string appName, string testSubfolderName, ProgramInfo pi = null
           , string viewName = null, bool waitForExit = true)
        {
            var appFile = TestAppProvider.ExtractApp(appName, testSubfolderName, x => IntegrationTestBase.Log(x));

            process = this.LaunchTestsApp(appFile, action, telemetryKey, pi, waitForExit, viewName);
            return appFile;
        }

        protected FileInfo LaunchTestsAppNewInstanceAndGetResult<T>(out Process process, out T result, Actions action, Guid telemetryKey, string appName, string testSubfolderName, ProgramInfo pi = null
            , string viewName = null, bool waitForExit = true) where T : class
        {
            var appFile = TestAppProvider.ExtractApp(appName, testSubfolderName, x => IntegrationTestBase.Log(x));


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
            IntegrationTestBase.Log($"Started process: {appFile.FullName}");
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            if (waitForExit)
            {
                process.WaitForExit();
                IntegrationTestBase.Log($"Finished process: {appFile.FullName}");
            }

            return process;
        }

        protected Process LaunchPackageUpdaterTestsAppWithArgs(out FileInfo appFile, string appName, string testSubfolderName, bool waitForExit)
        {
            appFile = TestAppProvider.ExtractApp(appName, testSubfolderName,x=> IntegrationTestBase.Log(x));
            Arguments args = new Arguments { ApiUrl = this.BaseUrl };

            args.TelemetryKey = Apps.Keys.PackageUpdaterClient;

            Process process = ProcessCreator.Create(appFile, args, this.outputs, this.errors);
            IntegrationTestBase.Log($"Started process: {appFile.FullName}");
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            if (waitForExit)
            {
                process.WaitForExit();
                IntegrationTestBase.Log($"Finished process: {appFile.FullName}");
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
                IntegrationTestBase.Log($"Failed to deserialize the outputs as {typeof(T).Name}");
            }
            return result;
        }
    }
}