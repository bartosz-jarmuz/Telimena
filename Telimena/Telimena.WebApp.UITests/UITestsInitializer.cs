using System.Drawing;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using Telimena.WebApp;
using Telimena.WebApp.UITests.Base;

[SetUpFixture]
public class TestInitializerInNoNamespace
{
    [OneTimeSetUp]
    public void Setup()
    {
        this.SetBrowser("Chrome");

    }

    public void SetBrowser(string browser)
    {
        switch (browser)
        {
            case "Chrome":
                var opt = new ChromeOptions();
#if DEBUG

#else
                opt.AddArgument("--headless");
#endif
                PortalTestBase.RemoteDriver = new ChromeDriver(opt);
                break;
            case "Firefox":
                PortalTestBase.RemoteDriver = new FirefoxDriver();
                break;
            case "IE":
                PortalTestBase.RemoteDriver = new InternetExplorerDriver();
                break;
            default:
                PortalTestBase.RemoteDriver = new ChromeDriver();
                break;
        }


    }

    [OneTimeTearDown]
    public void Teardown()
    {
        PortalTestBase.RemoteDriver.Dispose();

    }
}