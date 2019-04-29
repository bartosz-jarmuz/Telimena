using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

namespace Telimena.TestUtilities.Base
{
    public sealed class LocalHostTestEngine : ITestEngine
    {
        public LocalHostTestEngine()
        {
            this.portalAppName = "Telimena.WebApp";
            this.portalPort = 7757;
            this.BaseUrl = $"http://localhost:{this.portalPort}";
        }

        

        private readonly string portalAppName;
        private readonly int portalPort;
        private Process portalProcess;

        public string GetAbsoluteUrl(string relativeUrl)
        {
            if (!relativeUrl.StartsWith("/"))
            {
                relativeUrl = "/" + relativeUrl;
            }
            return $"{this.BaseUrl}/{relativeUrl}";
        }

        public string BaseUrl { get; }

        [TearDown]
        [TestCleanup]
        public void BaseCleanup()
        {
            // Ensure IISExpress is stopped
            if (this.portalProcess.HasExited == false)
            {
                this.portalProcess.Kill();
            }
        }

        [SetUp]
        [TestInitialize]
        public void BaseInitialize()
        {
            this.portalProcess = this.StartIIS(this.portalAppName, this.portalPort);

        }

        private string GetApplicationPath(string applicationName)
        {
            var solutionDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent;

            return Path.Combine(solutionDir.FullName, applicationName);
        }

        private Process StartIIS(string appName, int port)
        {
            string applicationPath = this.GetApplicationPath(appName);
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            Process p = new Process
            {
                StartInfo =
                {
                    FileName = programFiles + @"\IIS Express\iisexpress.exe",
                    Arguments = $"/path:\"{applicationPath}\" /port:{port}"
                }
            };
            p.Start();
            return p;
        }
    }
}