namespace Telimena.Client
{
    public class UpdatePackageData
    {
        public string StoredFilePath { get; set; }

        public string FileName { get; set; }
        public int Id { get; set; }
        public string Version { get; set; }

        public long FileSizeBytes { get; set; } 

        public bool IsStandalone { get; set; }
        public bool IsBeta { get; set; }
    }
}