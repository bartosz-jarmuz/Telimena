namespace Telimena.Client
{
    using System;

    public class StatisticsUpdateRequest 
    {
        public UserInfo UserInfo { get; set; }
        public ProgramInfo ProgramInfo { get; set; }
        public string TelimenaVersion { get; set; }
        public string FunctionName { get; set; }
    }

    public class StatisticsUpdateResponse : TelimenaResponseBase
    {
        public StatisticsUpdateResponse(Exception ex)
        {
            this.Exception = ex;
            this.IsMessageSuccessful = false;
        }

        public StatisticsUpdateResponse()
        {
        }

        public int Count { get; set; }
    }
}