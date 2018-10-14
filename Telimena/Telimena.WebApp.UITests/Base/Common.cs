using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace Telimena.WebApp.UITests.Base
{
    public static class Common
    {
        public static string TestOutputFolderPathBase => Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName, "Tests");

        public static string ScreenshotOutputFolderPath = Path.Combine(TestOutputFolderPathBase, "Selenium Screenshots");

        public static string CreatePngPath(string fileName)
        {
            Directory.CreateDirectory(Common.ScreenshotOutputFolderPath);
            return Path.Combine(Common.ScreenshotOutputFolderPath, fileName + ".png");
        }
        public static IWebElement GetElementByNameOrId(IWebDriver driver, string nameOrId)
        {
            try
            {
                return driver.FindElement(new ByIdOrName(nameOrId));
            }
            catch (Exception ex)
            {
                throw new AssertFailedException($"Failed to find element by name or id [{nameOrId}]", ex);
            }
        }
    }
}
