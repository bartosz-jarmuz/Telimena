using System.IO;
using TelimenaClient.Model;

namespace TelimenaClient.Tests
{
    internal class TestLocator : Locator
    {
        public TestLocator(ProgramInfo programInfo) : base(programInfo)
        {
        }

        internal TestLocator(ProgramInfo programInfo, string basePath) : base(programInfo, basePath)
        {
        }

        protected override DirectoryInfo GetAppDataFolder()
        {
            return new DirectoryInfo(Helpers.TestAppDataPath);
        }
    }
}