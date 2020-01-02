using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using Telimena.TestUtilities;
using Telimena.TestUtilities.Base;
using Telimena.WebApp.UiStrings;
using Assert = NUnit.Framework.Assert;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;
using TestContext = NUnit.Framework.TestContext;

namespace Telimena.WebApp.UITests
{
    [TestFixture]
    public abstract partial class WebsiteTestBase : IntegrationTestBase
    {
        public void WaitForSuccessConfirmationWithText(WebDriverWait wait, Action<string> validateText)
        {
            this.WaitForConfirmationWithTextAndClass(wait, "success", validateText);
        }

        public void WaitForErrorConfirmationWithText(WebDriverWait wait, Action<string> validateText)
        {
            this.WaitForConfirmationWithTextAndClass(wait, "danger", validateText);
        }

        private void WaitForConfirmationWithTextAndClass(WebDriverWait wait, string cssPart, Action<string> validateText)
        {
            var confirmationBox = this.Driver.TryFind(Strings.Id.TopAlertBox, (int)wait.Timeout.TotalMilliseconds);

            confirmationBox = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.TopAlertBox)));

            Assert.IsTrue(confirmationBox.GetAttribute("class").Contains(cssPart), "The alert has incorrect class: " + confirmationBox.GetAttribute("class"));
            validateText(confirmationBox.Text);
            //Assert.IsTrue(validateText(confirmationBox.Text), "Incorrect message: " + confirmationBox.Text);
            Log($"Confirmation [{cssPart}] box - Text: {confirmationBox.Text}");
        }

        public void WaitForPageLoad(int maxWaitTimeInSeconds)
        {
            string state = String.Empty;
            try
            {
                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(maxWaitTimeInSeconds));

                //Checks every 500 ms whether predicate returns true if returns exit otherwise keep trying till it returns ture
                wait.Until(d =>
                {

                    try
                    {
                        state = ((IJavaScriptExecutor) this.Driver).ExecuteScript(@"return document.readyState").ToString();
                    }
                    catch (InvalidOperationException)
                    {
                        //Ignore
                    }
                    catch (NoSuchWindowException)
                    {
                        //when popup is closed, switch to last windows
                        this.Driver.SwitchTo().Window(this.Driver.WindowHandles.Last());
                    }

                    //In IE7 there are chances we may get state as loaded instead of complete
                    return (state.Equals("complete", StringComparison.InvariantCultureIgnoreCase) ||
                            state.Equals("loaded", StringComparison.InvariantCultureIgnoreCase));

                });
            }
            catch (TimeoutException)
            {
                //sometimes Page remains in Interactive mode and never becomes Complete, then we can still try to access the controls
                if (!state.Equals("interactive", StringComparison.InvariantCultureIgnoreCase))
                    throw;
            }
            catch (NullReferenceException)
            {
                //sometimes Page remains in Interactive mode and never becomes Complete, then we can still try to access the controls
                if (!state.Equals("interactive", StringComparison.InvariantCultureIgnoreCase))
                    throw;
            }
            catch (WebDriverException)
            {
                if (this.Driver.WindowHandles.Count == 1)
                {
                    this.Driver.SwitchTo().Window(this.Driver.WindowHandles[0]);
                }

                state = ((IJavaScriptExecutor) this.Driver).ExecuteScript(@"return document.readyState").ToString();
                if (!(state.Equals("complete", StringComparison.InvariantCultureIgnoreCase) ||
                      state.Equals("loaded", StringComparison.InvariantCultureIgnoreCase)))
                    throw;
            }
        }

        protected bool IsLoggedIn()
        {
            if (this.Driver.TryFind(Strings.Id.MainHeader) != null)
            {
                return true;
            }

            return false;
        }
    }
}