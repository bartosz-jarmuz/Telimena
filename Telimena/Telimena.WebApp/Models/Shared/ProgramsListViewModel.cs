using System;
using System.Collections.Generic;

namespace Telimena.WebApp.Models.Shared
{
    public class ProgramsListViewModel
    {
        public Dictionary<Guid, Tuple<string, string>> Programs { get; set; } = new Dictionary<Guid, Tuple<string, string>>();
    }
}