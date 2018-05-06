namespace Telimena.Client
{
    public class StatisticsUpdateRequest 
    {
        public UserInfo UserInfo { get; set; }
        public ProgramInfo ProgramInfo { get; set; }
        public string TelimenaVersion { get; set; }
        public string FunctionName { get; set; }
    }

    public class StatisticsUpdateResponse : TelimenaResponseBase
    {
        public int Count { get; set; }
    }
}