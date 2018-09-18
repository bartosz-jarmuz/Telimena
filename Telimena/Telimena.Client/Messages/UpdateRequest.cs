using System;

namespace Telimena.Client
{
    public class UpdateRequest
    {
        [Obsolete("Only for serialization")]
        public UpdateRequest()
        {
        }

        public UpdateRequest(int programId, string programVersion, int userId, bool acceptBeta, string toolkitVersion)
        {
            this.ProgramId = programId;
            this.UserId = userId;
            this.ProgramVersion = programVersion;
            this.ToolkitVersion = toolkitVersion;
            this.AcceptBeta = acceptBeta;
        }

        public int ProgramId { get; set; }

        public int UserId { get; set; }

        public string ProgramVersion { get; set; }

        public string ToolkitVersion { get; set; }

        public bool AcceptBeta { get; set; }
    }
}