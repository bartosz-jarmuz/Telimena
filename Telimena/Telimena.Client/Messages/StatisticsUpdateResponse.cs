namespace Telimena.Client
{
    public class StatisticsUpdateResponse : TelimenaResponseBase
    {
        public string FunctionName { get; set; }
        public int ProgramId { get; set; }
        public int UserId { get; set; }
        public int Count { get; set; }
    }
}