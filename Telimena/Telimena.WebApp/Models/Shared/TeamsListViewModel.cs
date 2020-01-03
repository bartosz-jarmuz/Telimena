using System.Collections.Generic;

namespace Telimena.WebApp.Models.Shared
{
    public class TeamsListViewModel
    {
        public IEnumerable<TeamsMenuEntry> Teams { get; set; } = new List<TeamsMenuEntry>();
    }
}