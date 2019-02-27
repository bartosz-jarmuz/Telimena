using System;
using System.IO;

namespace Telimena.WebApp.Core.Models.Portal
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
            this.UploadedDate = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset UploadedDate { get; set; } 
        public string FileName { get; protected set; }

        public string ZippedFileName
        {
            get
            {
                if (this.FileName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
                {
                    return this.FileName;
                }
                else
                {
                    return Path.GetFileNameWithoutExtension(this.FileName) + ".zip";
                }
            }
        }

        public string FileLocation { get; set; }
        public long FileSizeBytes { get;  set; }
    }
}