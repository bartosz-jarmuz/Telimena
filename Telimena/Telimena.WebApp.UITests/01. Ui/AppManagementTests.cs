using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Telimena.TestUtilities.Base;
using Telimena.TestUtilities.Base.TestAppInteraction;
using Telimena.WebApp.UiStrings;
using TelimenaClient;
using TelimenaClient.Model;
using static System.Reflection.MethodBase;

namespace Telimena.WebApp.UITests._01._Ui
{
    using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

    [TestFixture, Order(1)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class _1_WebsiteTests : WebsiteTestBase
    {
        [Test, Order(4), Retry(3)]
        public void _04_RegisterAutomaticTestsClient()
        {
            try
            {
                this.RegisterApp(Apps.Names.AutomaticTestsClient, Apps.Keys.AutomaticTestsClient, "Telimena system tests app", "AutomaticTestsClient.exe", true, false);
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }

        [Test, Order(4), Retry(3)]
        public void _04b_RegisterAutomaticTestsClient_PackageUpdaterTest()
        {
            try
            {
                this.RegisterApp(Apps.Names.PackageUpdaterTest, Apps.Keys.PackageUpdaterClient, 
                    "Telimena package updater test app (for apps where update package is an actual installer)", "PackageTriggerUpdaterTestApp.exe", true, false);
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }

        [Test, Order(4), Retry(3)]
        public void _04c_RegisterInstallerTestApp_Msi3()
        {
            try
            {
                this.RegisterApp(Apps.Names.InstallersTestAppMsi3, Apps.Keys.InstallersTestAppMsi3,
                    "Telimena MSI installer test app", "InstallersTestApp.exe", true, false);
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }

        [Test, Retry(3)]
        public void RegisterTempTestApp()
        {
            try
            {
                string appName = "Unit-Test-App";

                this.DeleteApp(appName, true);


                this.RegisterApp(appName, null, "To be deleted", "Auto test TestPlugin.dll", true, false);

                this.RegisterApp(appName, null, "To be deleted", "Auto test TestPlugin.dll", true, true);

                this.DeleteApp(appName, false);
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
            finally
            {
                var alert = this.Driver.WaitForAlert(500);
                alert?.Dismiss();
            }
        }

        

   

        [Test, Order(7), Retry(3)]
        public void _07_SetUpdaterForPackageTriggerApp()
        { 

            var app = Apps.Names.PackageUpdaterTest;
            var currentUpdater = this.GetUpdaterForApp(app);

            if (currentUpdater == DefaultToolkitNames.PackageTriggerUpdaterInternalName)
            {
                this.SetUpdaterForApp(app, DefaultToolkitNames.UpdaterInternalName);
                Assert.AreEqual(DefaultToolkitNames.UpdaterInternalName, this.GetUpdaterForApp(app));

                this.SetUpdaterForApp(app, DefaultToolkitNames.PackageTriggerUpdaterInternalName);
                Assert.AreEqual(DefaultToolkitNames.PackageTriggerUpdaterInternalName, this.GetUpdaterForApp(app));
            }
            else
            {
                this.SetUpdaterForApp(app, DefaultToolkitNames.PackageTriggerUpdaterInternalName);
                Assert.AreEqual(DefaultToolkitNames.PackageTriggerUpdaterInternalName, this.GetUpdaterForApp(app));
            }
        }

        [Test, Order(8), Retry(3)]
        public void _08_SetNotesOnExistingPackage()
        {
            try
            {

                this.LoginAdminIfNeeded();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            
                this.ClickOnManageProgramMenu(Apps.Names.AutomaticTestsClient);

                var notes = GetCurrentMethod().Name + DateTimeOffset.UtcNow.ToString("O");

                this.SetReleaseNotesOnExistingPkg( notes);

                this.WaitForSuccessConfirmationWithText(wait, z=>z.Contains("Updated release notes."));

                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.Id(Strings.Id.TopAlertBox)));

                //wait for the reload and verify package uploaded and notes set OK
                this.VerifyReleaseNotesOnPkg( notes);

            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }

        }

     
        [Test, Order(10), Retry(3)]
        public void _10_DownloadInstallerTestAppMsi3Package()
        {
            this.UninstallPackages(Apps.ProductCodes.InstallersTestAppMsi3V1, Apps.ProductCodes.InstallersTestAppMsi3V2);

            this.Log("Proceeding to download");

            this.DownloadAndInstallMsiProgramPackage(Apps.Names.InstallersTestAppMsi3, Apps.PackageNames.InstallersTestAppMsi3V1);

            this.Log("Uninstalling packages after test");
            this.UninstallPackages(Apps.ProductCodes.InstallersTestAppMsi3V1, Apps.ProductCodes.InstallersTestAppMsi3V2);
        }

        [Test, Order(12), Retry(3)]
        public void _12_SetUpdaterForMsiInstallerApp()
        {

            var app = Apps.Names.InstallersTestAppMsi3;
            var currentUpdater = this.GetUpdaterForApp(app);

            if (currentUpdater == DefaultToolkitNames.PackageTriggerUpdaterInternalName)
            {
                //ok
            }
            else
            {
                this.SetUpdaterForApp(app, DefaultToolkitNames.PackageTriggerUpdaterInternalName);
                Assert.AreEqual(DefaultToolkitNames.PackageTriggerUpdaterInternalName, this.GetUpdaterForApp(app));
            }
        }

    }


}