#pragma warning disable 1591

using System.Collections.Generic;
using System.Linq;

namespace Telimena.WebApp.Models.Shared
{
    public class ProgramsListViewModel
    {
        public IEnumerable<IGrouping<int, ProgramMenuEntry>> Programs { get; set; } = new List<IGrouping<int, ProgramMenuEntry>>();
    }
}
