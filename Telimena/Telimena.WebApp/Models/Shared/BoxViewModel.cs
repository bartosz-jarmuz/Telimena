using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Telimena.WebApp.Models.Shared
{
    public class BoxViewModel
    {
        public string MoreInfoUrl { get; set; }
        public string BackgroundColorClass { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }
        public string SecondaryLabel { get; set; }
        public string IconClass { get; set; }
    }
}