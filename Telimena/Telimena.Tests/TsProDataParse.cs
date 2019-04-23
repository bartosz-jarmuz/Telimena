#if DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using Telimena.WebApp.Core.DTO;

namespace Telimena.Tests
{
    [TestFixture]
    public class TsProDataParse

    {

        [Test]
        public void ValidateExpandoObject()
        {
            var path = @"C:\Users\bjarmuz\Downloads\2019-02-06 20-58-44_FunctionsCustomDataExport_TimesheetPro.json";

            var text = File.ReadAllText(path);

            var deserialized = JsonConvert.DeserializeObject<Data>(text);

            StringBuilder sb = new StringBuilder();
            var _ = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;

            sb.AppendLine($"Date{_}Time{_}User{_}Function");
            foreach (DataElement data in deserialized.Elements)
            {
                sb.AppendLine($"{data.GenericData.DateTime.Date}{_}{data.GenericData.DateTime:HH:mm:ss}{_}{data.GenericData.UserName}{_}{data.GenericData.FunctionName}");
            }
            File.WriteAllText(path+".csv", sb.ToString());
        }


    }

    public class Data
    {
        [JsonProperty("data")]
        public DataElement[] Elements{ get; set; }

    }


    public partial class DataElement
    {
        [JsonProperty("customData")]
        public CustomData CustomData { get; set; }

        [JsonProperty("genericData")]
        public GenericData GenericData { get; set; }
    }

    public partial class CustomData
    {
        [JsonProperty("DurationMinutes")]
        public long DurationMinutes { get; set; }
    }

    public partial class GenericData
    {
        [JsonProperty("DateTime")]
        public DateTimeOffset DateTime { get; set; }

        [JsonProperty("detailId")]
        public long DetailId { get; set; }

        [JsonProperty("programVersion")]
        public string ProgramVersion { get; set; }

        [JsonProperty("functionName")]
        public string FunctionName { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("userId")]
        public long UserId { get; set; }
    }
}
#endif
