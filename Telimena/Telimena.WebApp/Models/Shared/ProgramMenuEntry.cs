// -----------------------------------------------------------------------
//  <copyright file="ProgramMenuEntry.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;

namespace Telimena.WebApp.Models.Shared
{
    public class ProgramMenuEntry
    {
        public string ProgramName { get; set; }
        public string ProgramNameTrimmed 
        {
            get
            {
                if (this.ProgramName.Length > 24)
                {
                    return this.ProgramName.Remove(22) + "...";
                }

                return this.ProgramName;
            }
        }

        public Guid TelemetryKey { get; set; }

        public string DeveloperTeamName { get; set; }

        public int DeveloperTeamId { get; set; }
    }
}