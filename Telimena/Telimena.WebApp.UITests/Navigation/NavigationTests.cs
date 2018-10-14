//using System;
//using NUnit.Framework;
//using OpenQA.Selenium;
//using OpenQA.Selenium.Support.PageObjects;
//using OpenQA.Selenium.Support.UI;
//using Telimena.WebApp.UITests.Base;

//namespace Telimena.WebApp.UITests.Navigation
//{
//    public class NavigationTests : PortalTestBase
//    {
//        [Test]
//        public void GoThroughAllPagesAsAdmin()
//        {
//            try
//            {
//                this.GoToAdminHomePage();

//                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

//                this.Driver.FindElement(By.Id("dashboardLink")).Click();
//                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@id='page-wrapper']/div/div/h1[text()='Dashboard']")));

//                this.Driver.FindElement(By.Id("adHocMtLink")).Click();
//                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("translateString")));

//                this.Driver.FindElement(By.Id("projectsLink")).Click();
//                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("projectsList")));

//                this.Driver.FindElement(By.Id("createProjectLink")).Click();
//                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("createProject")));

//                this.Driver.FindElement(By.XPath("//ul[@id='side-menu']/li/a[@href='#']")).Click();

//                IWebElement userDetailsLink = wait.Until(x => x.FindElement(By.Id("userDetailsLink")));
//                wait.Until(x => userDetailsLink.Displayed);
//                userDetailsLink.Click();
//                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("userDetails")));

//                IWebElement teamLeaderLink = wait.Until(x => x.FindElement(By.Id("teamLeaderPanelLink")));
//                teamLeaderLink.Click();
//                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("teamLeaderPanel")));

//                IWebElement adminPanelLink = wait.Until(x => x.FindElement(By.Id("adminPanelLink")));
//                adminPanelLink.Click();
//                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("adminPanel")));

//                IWebElement logViewerLink = wait.Until(x => x.FindElement(By.Id("logViewerLink")));
//                logViewerLink.Click();
//                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("logViewerPanel")));
//            }
//            catch (Exception ex)
//            {
//                this.HandlerError(ex);
//            }
//        }

//        [Test]
//        public void AdminLogin_Failed()
//        {
//            try
//            {
//                this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl(""));

//                IWebElement login = this.Driver.FindElement(new ByIdOrName("email"));
//                IWebElement pass = this.Driver.FindElement(new ByIdOrName("password"));
//                login.SendKeys("Wrong");
//                pass.SendKeys("Dude");
//                IWebElement submit = this.Driver.FindElement(new ByIdOrName("submitLogin"));
//                submit.Click();
//                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
//                IWebElement element = wait.Until(x => x.FindElement(By.ClassName("validation-summary-errors")));

//                Assert.AreEqual("Invalid username or password", element.Text);
//            }
//            catch (Exception ex)
//            {
//                this.HandlerError(ex);
//            }
//        }

//    }
//}
