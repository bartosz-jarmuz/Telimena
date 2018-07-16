using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.WebApp.Core.Models
{
    public class UpdatePackage : RepositoryFileBase, IRepositoryFile
    {
        protected UpdatePackage() : base(){ }
        public UpdatePackage(string fileName, int programId, string version, long fileSizeBytes) : base(fileName, fileSizeBytes)
        {
            this.ProgramId = programId;
            this.Version = version;
        }

        public int Id { get; set; }
        public int ProgramId { get; set; }
        public string Version { get; set; }
    }
}
