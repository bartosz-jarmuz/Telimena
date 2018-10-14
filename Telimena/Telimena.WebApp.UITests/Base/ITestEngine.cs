namespace Telimena.WebApp.UITests.Base
{
    public interface ITestEngine
    {
        void BaseCleanup();
        void BaseInitialize();
        string GetAbsoluteUrl(string relativeUrl);
    }
}