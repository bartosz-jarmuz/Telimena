namespace Telimena.WebApp.Core.Models
{
    public interface IRepositoryFile
    {
        string FileName { get; }
        string FileLocation { get; set; }
        long FileSizeBytes { get; }
    }
}