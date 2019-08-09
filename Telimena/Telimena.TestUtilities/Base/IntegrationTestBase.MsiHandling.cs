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

        public void InstallMsi(FileInfo msi, FileInfo expectedProgramPath)
        {
            var productCode = this.GetCodeFromMsi(msi);
            Log($"Installing product {productCode}. Installer path: {msi.FullName}.");

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
                Log($"Uninstalled package {productCode}.");

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
                    Log($"Product {productCode} cannot be uninstalled because it is not installed at the moment.");
                }
            }
        }

    }
}