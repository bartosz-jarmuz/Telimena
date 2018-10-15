using System.Configuration;

namespace Telimena.WebApp.UITests.Base
{
    public sealed class DeployedTestEngine : ITestEngine
    {

        private readonly string portalUrl;

        public DeployedTestEngine(string portalUrl)
        {
            this.portalUrl = portalUrl;
        }

        public string GetAbsoluteUrl(string relativeUrl)
        {
            if (!relativeUrl.StartsWith("/"))
            {
                relativeUrl = "/" + relativeUrl;
            }
            return $"{this.portalUrl.TrimEnd('/')}/{relativeUrl}";
        }

        public void BaseInitialize()
        {
        }

        public void BaseCleanup()
        {
        }
    }
}