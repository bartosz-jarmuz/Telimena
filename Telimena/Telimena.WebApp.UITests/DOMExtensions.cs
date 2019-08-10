using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DotNetLittleHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using Telimena.TestUtilities.Base;

namespace Telimena.WebApp.UITests
{
    public static class DOMExtensions
    {
        public static IWebElement TryFind(this IWebDriver driver, string id, int timeoutSeconds = 10)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
                var ele = wait.Until(x => x.FindElement(By.Id(id)));
                driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", ele);
                return ele;
            }
            catch
            {
                IntegrationTestBase.Log($"Failed to find element by Id {id}");
            }

            return null;
        }

        public static IWebElement TryFind(this IWebDriver driver, By by,  int timeoutSeconds = 10)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
                var element = wait.Until(x => x.FindElement(@by));
                Actions actions = new Actions(driver);
                actions.MoveToElement(element);
                actions.Perform();
                driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", element);

                return element;
            }
            catch
            {
                IntegrationTestBase.Log($"Failed to find element using By {@by.ToString()}");
            }

            return null;
        }

        public static ReadOnlyCollection<IWebElement> TryFindMany(this IWebDriver driver, By by, int timeoutSeconds = 10)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
                var elements = wait.Until(x => x.FindElements(@by));
                if (elements.Any())
                {

                    Actions actions = new Actions(driver);
                    actions.MoveToElement(elements.Last());
                    actions.Perform();
                }

                return elements;
            }
            catch
            {
            }

            IntegrationTestBase.Log("Failed to find elements");
            return null;
        }

        public static IWebElement TryFind(Func<IWebElement> finderFunc, TimeSpan timeout)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeout.TotalMilliseconds)
            {
                try
                {
                    IWebElement result = finderFunc();
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch
                {
                    //np
                }
            }

            IntegrationTestBase.Log($"Failed to find object within {timeout.TotalMilliseconds}");
            return null;
        }

        public static IWebElement GetElementByNameOrId(this IWebDriver driver, string nameOrId)
        {
            try
            {
                return driver.FindElement(new ByIdOrName(nameOrId));
            }
            catch (Exception ex)
            {
                throw new TelimenaTestException($"Failed to find element by name or id [{nameOrId}]", ex);
            }
        }

        public static IAlert WaitForAlert(this IWebDriver driver, int timeout, bool obligatory = false)
        {

            var sw = Stopwatch.StartNew();
            while (true)
            {

                try
                {
                    var alert = driver.SwitchTo().Alert();
                    return alert;
                }
                catch (NoAlertPresentException)
                {
                    if (sw.ElapsedMilliseconds < timeout)
                    {
                        continue;
                    }

                    if (obligatory)
                    {
                        throw;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public static void ClickWrapper(this IWebElement element, IWebDriver driver)
        {
            int attempt = 0;
            string props = element?.GetPropertyInfoString();
            while (attempt < 4)
            {
                attempt++;
                try
                {

                    element.Click();
                    IntegrationTestBase.Log($"Click Attempt {attempt} - Successfully clicked on element: {props}");
                    return;
                }
                catch (Exception ex)
                {
                    IntegrationTestBase.Log($"Click Attempt {attempt} - Could not click on element: {props}. {ex}");
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(element).Click().Perform();
                    actions.Perform();
                    driver.ExecuteJavaScript("arguments[0].scrollIntoView(true);", element);
                    Thread.Sleep(500);

                    if (element != null)
                    {

                        try
                        {
                            element.Click();
                        }
                        catch (WebDriverException anotherEx)
                        {
                            if (attempt >= 3)
                            {
                                throw new TelimenaTestException($"Cannot click on [{element?.TagName}] element: [{element?.Text}] regardless of moving it to viewport." +
                                                                    anotherEx);
                            }
                            else
                            {
                                IntegrationTestBase.Log($"Click Attempt {attempt} - Could not click on [{element?.TagName}] element: [{element?.Text}] Regardless of moving to viewport. {ex}");
                            }

                        }
                    }
                }
            }
        }
    }
}