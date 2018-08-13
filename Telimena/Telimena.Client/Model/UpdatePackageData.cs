namespace Telimena.Client
{
    public class UpdatePackageData
    {
        public int Id { get; set; }

        public long FileSizeBytes { get; set; } 

        public bool IsStandalone { get; set; }
        public bool IsBeta { get; set; }
    }
}