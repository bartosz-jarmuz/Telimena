#pragma warning disable 1591

using System;
using System.Collections.Generic;

namespace Telimena.WebApp.Models.Shared
{
    public class ProgramsListViewModel
    {
        public Dictionary<Guid, string> Programs { get; set; } = new Dictionary<Guid,string>();
    }
}