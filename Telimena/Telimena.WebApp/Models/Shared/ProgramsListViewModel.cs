using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Telimena.WebApp.Models.Shared
{
    public class ProgramsListViewModel
    {
        public Dictionary<int, string> Programs { get; set; } = new Dictionary<int, string>();
    }
}