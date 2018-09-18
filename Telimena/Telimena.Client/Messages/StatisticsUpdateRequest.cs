namespace Telimena.Client
{
    public class StatisticsUpdateRequest
    {
        public int UserId { get; set; }
        public int ProgramId { get; set; }
        public string FunctionName { get; set; }
        public string Version { get; set; }
    }
}