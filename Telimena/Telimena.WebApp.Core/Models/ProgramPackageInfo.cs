namespace Telimena.WebApp.Core.Models
{
    public class ProgramPackageInfo : RepositoryFileBase, IRepositoryFile
    {
        public int Id { get; set; }
        public int ProgramId { get; set; }
        protected ProgramPackageInfo() : base() { }

        public ProgramPackageInfo(string fileName, int programId, long fileSizeBytes) : base(fileName, fileSizeBytes)
        {
            this.ProgramId = programId;
        }
        
    }
}