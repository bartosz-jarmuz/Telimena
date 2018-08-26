using System.Collections.Generic;

namespace Telimena.Updater
{
    public class UpdateInstructions
    {

        public string ProgramExecutableLocation { get; set; }

        public string LatestVersion { get; set; }

        public List<string> PackagePaths { get; set; }
    }
}