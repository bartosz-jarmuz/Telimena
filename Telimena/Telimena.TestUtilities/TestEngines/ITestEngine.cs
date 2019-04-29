namespace Telimena.TestUtilities.Base
{
    public interface ITestEngine
    {
        string BaseUrl { get; }

        void BaseCleanup();
        void BaseInitialize();
        string GetAbsoluteUrl(string relativeUrl);
    }
}