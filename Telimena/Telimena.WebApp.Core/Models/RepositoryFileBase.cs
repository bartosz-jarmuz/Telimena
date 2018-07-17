namespace Telimena.WebApp.Core.Models
{
    using System;

    public abstract class RepositoryFileBase : IRepositoryFile
    {
        protected RepositoryFileBase()
        {
        }

        protected RepositoryFileBase(string fileName, long fileSizeBytes)
        {
            this.FileName = fileName;
            this.FileSizeBytes = fileSizeBytes;
            this.UploadedDate = DateTime.UtcNow;
        }


        public DateTime UploadedDate { get; set; }
        public string FileName { get; protected set; }
        public string FileLocation { get; set; }
        public long FileSizeBytes { get; protected set; }
    }
}