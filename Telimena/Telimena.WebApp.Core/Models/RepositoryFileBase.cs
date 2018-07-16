namespace Telimena.WebApp.Core.Models
{
    public abstract class RepositoryFileBase : IRepositoryFile
    {
        protected RepositoryFileBase()
        {
        }

        protected RepositoryFileBase(string fileName, long fileSizeBytes)
        {
            this.FileName = fileName;
            this.FileSizeBytes = fileSizeBytes;
        }

        public string FileName { get; protected set; }
        public string FileLocation { get; set; }
        public long FileSizeBytes { get; protected set; }
    }
}