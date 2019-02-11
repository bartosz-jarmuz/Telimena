using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Core.DTO
{
    public class DataTableTelemetryDataBase
    {
        public DateTimeOffset Timestamp { get; set; }
        public string UserName { get; set; }
        public string EntryKey { get; set; }
        public string ProgramVersion { get; set; }
        public string Sequence { get; set; }
        public string IpAddress { get; set; }

    }

    public class SequenceHistoryData : DataTableTelemetryDataBase
    {
        public SequenceHistoryData()
        {
        }

        private void LoadBaseItems(DateTimeOffset timestamp, string key, string itemType)
        {
            this.DataType = itemType;
            this.Timestamp = timestamp;
            this.EntryKey = key;

        }

        public SequenceHistoryData(TelemetryDetail detail)
        {
            this.LoadBaseItems(detail.Timestamp, detail.EntryKey, GetTypeName(detail));
            this.Values = detail.GetTelemetryUnits().ToDictionary(x => x.Key, x => x.ValueString);
        }

        public SequenceHistoryData(LogMessage item)
        {
            this.LoadBaseItems(item.Timestamp, item.Level.ToString(), GetTypeName(item));
            this.Message = item.Message;
        }

        public SequenceHistoryData(ExceptionInfo exception)
        {
            this.LoadBaseItems(exception.Timestamp, exception.TypeName, GetTypeName(exception));
            this.Message = exception.Message;
            this.StackTrace = this.GetStackTrace(exception.ParsedStack);
        }

        private static string GetTypeName(object input)
        {
            if (input is ViewTelemetryDetail)
            {
                return "View";
            }

            if (input is EventTelemetryDetail)
            {
                return "Event";
            }

            if (input is ExceptionInfo)
            {
                return "Exception";
            }

            if (input is LogMessage)
            {
                return "LogMessage";
            }

            return input.GetType().Name;
        }



        private List<TelemetryItem.ExceptionInfo.ParsedStackTrace> GetStackTrace(string input)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<TelemetryItem.ExceptionInfo.ParsedStackTrace>>(input);
            }
            catch
            {
                //failsafe approach
                return new List<TelemetryItem.ExceptionInfo.ParsedStackTrace>() { new TelemetryItem.ExceptionInfo.ParsedStackTrace() { Method = input } };
            }
        }


        public int Order { get; set; }
        public string DataType { get; set; }
        public string Message { get; set; }
        public List<TelemetryItem.ExceptionInfo.ParsedStackTrace> StackTrace { get; set; } = new List<TelemetryItem.ExceptionInfo.ParsedStackTrace>();
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

    }

    public class DataTableTelemetryData : DataTableTelemetryDataBase
    {
        public Dictionary<string, string> Values { get; set; } =new Dictionary<string, string>();
    }

    public class LogMessageData : DataTableTelemetryDataBase
    {
        public string Message { get; set; }
        public string LogLevel{ get; set; }
    }

    public class ExceptionData : DataTableTelemetryDataBase
    {
        public string ErrorMessage { get; set; }
        public List<TelemetryItem.ExceptionInfo.ParsedStackTrace> StackTrace { get; set; } = new List<TelemetryItem.ExceptionInfo.ParsedStackTrace>();
    }
}