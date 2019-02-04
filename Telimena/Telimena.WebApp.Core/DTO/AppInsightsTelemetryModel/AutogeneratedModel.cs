using System;
using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo



//******************
//Generated based on json string using converter:
//https://app.quicktype.io
// Don't change unless the json changes as well

namespace Telimena.WebApp.Core.DTO.AppInsightsTelemetryModel
{
    /// <summary>
    /// Class AppInsightsTelemetry.
    /// </summary>
    public partial class AppInsightsTelemetry
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("time")]
        public DateTimeOffset Time { get; set; }

        [JsonProperty("seq")]
        public string Seq { get; set; }

        [JsonProperty("iKey")]
        public Guid IKey { get; set; }

        [JsonProperty("tags")]
        public Tags Tags { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("baseType")]
        public string BaseType { get; set; }

        [JsonProperty("baseData")]
        public BaseData BaseData { get; set; }
    }

    public partial class BaseData
    {
        [JsonProperty("ver")]
        public long Ver { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("severityLevel")]
        public string SeverityLevel { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, string> Properties { get; set; }
        [JsonProperty("exceptions")]
        public ExceptionElement[] Exceptions { get; set; }

    }

    public partial class ExceptionElement
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("outerId")]
        public long OuterId { get; set; }

        [JsonProperty("typeName")]
        public string TypeName { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("hasFullStack")]
        public bool HasFullStack { get; set; }

        [JsonProperty("parsedStack")]
        public ParsedStack[] ParsedStack { get; set; }
    }

    public partial class ParsedStack
    {
        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("assembly")]
        public string Assembly { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("line")]
        public long Line { get; set; }
    }




    public partial class Tags
    {
        [JsonProperty("ai.device.osVersion")]
        public string AiDeviceOsVersion { get; set; }

        [JsonProperty("ai.cloud.roleInstance")]
        public string AiCloudRoleInstance { get; set; }

        [JsonProperty("ai.user.id")]
        public string AiUserId { get; set; }

        [JsonProperty("ai.user.authUserId")]
        public string AiUserAuthUserId { get; set; }

        [JsonProperty("ai.internal.sdkVersion")]
        public string AiInternalSdkVersion { get; set; }
    }
}
