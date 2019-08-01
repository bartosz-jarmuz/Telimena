using System;

namespace Telimena.WebApp.Core.Models.Portal
{
    public interface IRepositoryFile
    {
        string FileName { get; }
        string FileLocation { get; set; }
        long FileSizeBytes { get; }
        Guid PublicId { get; set; } 
    }
}