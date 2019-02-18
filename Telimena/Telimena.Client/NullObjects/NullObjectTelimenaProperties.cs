using System;
using TelimenaClient.Model;
using TelimenaClient.Model.Internal;
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace TelimenaClient
{
    internal class NullObjectTelimenaProperties : ITelimenaProperties
    {
        public bool SuppressAllErrors { get; set; } = true;
        public Guid TelemetryKey { get; }
        public UserInfo UserInfo { get; } = new UserInfo();
        public ProgramInfo StaticProgramInfo { get; } = new ProgramInfo();
        public VersionData ProgramVersion { get; } = new VersionData("0.0.0.0", "0.0.0.0");
        public string TelimenaVersion { get; } = "0.0.0.0";
        public LiveProgramInfo LiveProgramInfo { get; } 
        public Uri TelemetryApiBaseUrl { get; }
        public string InstrumentationKey { get; } = "NullObject";
        public ITelimenaStartupInfo StartupInfo { get; }
    }
}