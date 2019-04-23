using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Telemetry;

namespace Telimena.WebApp.Core.DTO
{
    public class DataTableTelemetryData
    {
        public DateTimeOffset Timestamp { get; set; }
        public string UserName { get; set; }
        public string EntryKey { get; set; }
        public string ProgramVersion { get; set; }
        public string Sequence { get; set; }
        public string IpAddress { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, double> Metrics { get; set; } = new Dictionary<string, double>();

    }

    public class SequenceHistoryData : DataTableTelemetryData
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
            this.Properties = detail.GetTelemetryUnits().Where(x => x.UnitType == TelemetryUnit.UnitTypes.Property)
                .ToDictionary(x => x.Key, x => x.ValueString);
            this.Metrics = detail.GetTelemetryUnits().Where(x => x.UnitType == TelemetryUnit.UnitTypes.Metric)
                .ToDictionary(x => x.Key, x => x.ValueDouble);
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

    }

   

    public class LogMessageData : DataTableTelemetryData
    {
        public string Message { get; set; }
        public string LogLevel{ get; set; }
    }

    public class ExceptionData : DataTableTelemetryData
    {
        public string ErrorMessage { get; set; }

        public string Note { get; set; }
        public List<TelemetryItem.ExceptionInfo.ParsedStackTrace> StackTrace { get; set; } = new List<TelemetryItem.ExceptionInfo.ParsedStackTrace>();
    }
}