using System.Collections.Generic;

namespace Telimena.WebApp.Models.Shared
{
    public class ProgramsListViewModel
    {
        public Dictionary<int, string> Programs { get; set; } = new Dictionary<int, string>();
    }
}