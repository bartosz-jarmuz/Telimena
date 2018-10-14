using System.Configuration;

namespace Telimena.WebApp.UITests.Base
{
    public sealed class DeployedTestEngine : ITestEngine
    {
        public readonly string PortalUrl = ConfigurationManager.AppSettings.Get(ConfigKeys.PortalUrl);

        public string GetAbsoluteUrl(string relativeUrl)
        {
            if (!relativeUrl.StartsWith("/"))
            {
                relativeUrl = "/" + relativeUrl;
            }
            return $"{this.PortalUrl.TrimEnd('/')}/{relativeUrl}";
        }

        public void BaseInitialize()
        {
        }

        public void BaseCleanup()
        {
        }
    }
}