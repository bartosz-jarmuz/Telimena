namespace Telimena.TestUtilities.Base
{
    public sealed class DeployedTestEngine : ITestEngine
    {


        public DeployedTestEngine(string portalUrl)
        {
            this.BaseUrl = portalUrl.TrimEnd('/');

        }



        public string GetAbsoluteUrl(string relativeUrl)
        {
            if (!relativeUrl.StartsWith("/"))
            {
                relativeUrl = "/" + relativeUrl;
            }
            return $"{this.BaseUrl}/{relativeUrl}";
        }

        public void BaseInitialize()
        {
        }

        public string BaseUrl { get; }

        public void BaseCleanup()
        {
        }
    }
}