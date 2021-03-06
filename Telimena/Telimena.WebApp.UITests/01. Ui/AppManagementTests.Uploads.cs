﻿using System;
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

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class _1_WebsiteTests : WebsiteTestBase
    {

        [Test, Order(5), Retry(3)]
        public void _05_UploadTestAppUpdate()
        {
            try
            {
                this.UploadUpdatePackage(Apps.Keys.AutomaticTestsClient, Apps.PackageNames.AutomaticTestsClientAppV2);
                this.SetPackageAsNonBeta(Apps.PackageNames.AutomaticTestsClientAppV2);

            }
            catch (Exception ex)
            {
                this.HandleError(ex, SharedTestHelpers.GetMethodName());
            }
        }

        [Test, Order(5), Retry(3)]
        public void _05_UploadTestAppUpdate_Beta()
        {
            try
            {
                this.UploadUpdatePackage(Apps.Keys.AutomaticTestsClient, Apps.PackageNames.AutomaticTestsClientAppV3Beta);
                this.SetPackageAsBeta(Apps.PackageNames.AutomaticTestsClientAppV3Beta);
            }
            catch (Exception ex)
            {
                this.HandleError(ex, SharedTestHelpers.GetMethodName());
            }
        }

        [Test, Order(5), Retry(3)]
        public void _05c_UploadTestAppPackage()
        {
            try
            {
                this.UploadProgramPackage(Apps.Keys.AutomaticTestsClient, Apps.PackageNames.AutomaticTestsClientAppV1);
            }
            catch (Exception ex)
            {
                this.HandleError(ex, SharedTestHelpers.GetMethodName());
            }
        }

        [Test, Order(6), Retry(3)]
        public void _06_UploadPackageUpdateTestAppUpdate()
        {
            try
            {
                this.UploadUpdatePackage(Apps.Keys.PackageUpdaterClient, Apps.PackageNames.PackageUpdaterTestAppV2);
                this.SetPackageAsNonBeta(Apps.PackageNames.PackageUpdaterTestAppV2);

            }
            catch (Exception ex)
            {
                this.HandleError(ex, SharedTestHelpers.GetMethodName());
            }
        }

        [Test, Order(9), Retry(3)]
        public void _09_UploadInstallerTestAppMsi3Package()
        {
            try
            {
                Assert.AreEqual(Apps.ProductCodes.InstallersTestAppMsi3V1
                    , this.GetCodeFromMsi(Apps.PackageNames.InstallersTestAppMsi3V1));
                this.UploadProgramPackage(Apps.Keys.InstallersTestAppMsi3, Apps.PackageNames.InstallersTestAppMsi3V1);

            }
            catch (Exception ex)
            {
                this.HandleError(ex, SharedTestHelpers.GetMethodName());
            }
        }

        [Test, Order(11), Retry(3)]
        public void _11_UploadInstallerMsi3TestAppUpdate()
        {
            try
            {
                Assert.AreEqual(Apps.ProductCodes.InstallersTestAppMsi3V2
                    , this.GetCodeFromMsi(Apps.PackageNames.InstallersTestAppMsi3V2));
                this.UploadUpdatePackage(Apps.Keys.InstallersTestAppMsi3, Apps.PackageNames.InstallersTestAppMsi3V2);
                this.SetPackageAsNonBeta(Apps.PackageNames.InstallersTestAppMsi3V2);

            }
            catch (Exception ex)
            {
                this.HandleError(ex, SharedTestHelpers.GetMethodName());
            }
        }
    }
}