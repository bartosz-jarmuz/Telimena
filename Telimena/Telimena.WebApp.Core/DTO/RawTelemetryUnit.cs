// -----------------------------------------------------------------------
//  <copyright file="RawTelemetryUnit.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;

namespace Telimena.WebApp.Core.DTO
{
    public class RawTelemetryUnit
    {
        public string Sequence { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string User { get; set; }
        public string ComponentName { get; set; }
        public string Key { get; set; }
        public string PropertyValue { get; set; }
        public double MetricValue { get; set; }
    }
}