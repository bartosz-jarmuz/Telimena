using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
#pragma warning disable 1591

namespace Telimena.WebApp.Models.Team
{
    public class TeamViewModel
    {
        public string Name { get; set; }

        public List<SelectListItem> TeamMembers { get; set; } = new List<SelectListItem>();

    }
}