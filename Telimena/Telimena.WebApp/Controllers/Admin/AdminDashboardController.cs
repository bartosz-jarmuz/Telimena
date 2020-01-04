using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Mvc;
using DataTables.AspNet.Core;
using DataTables.AspNet.Mvc5;
using DotNetLittleHelpers;
using Newtonsoft.Json;
using Telimena.WebApp.Controllers.Developer;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Core.Models.Telemetry;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Security;

namespace Telimena.WebApp.Controllers.Admin
{
    /// <summary>
    /// Class AdminDashboardController.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    [TelimenaAuthorize(Roles = TelimenaRoles.Admin)]
    public class AdminDashboardController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminDashboardController"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public AdminDashboardController( IProgramsDashboardUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            this.dashboardBase = new ProgramsDashboardBase(unitOfWork);
        }

        /// <summary>
        /// The unit of work
        /// </summary>
        private readonly IProgramsDashboardUnitOfWork unitOfWork;
        /// <summary>
        /// The dashboard base
        /// </summary>
        private readonly ProgramsDashboardBase dashboardBase;

        /// <summary>
        /// Gets all programs.
        /// </summary>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [HttpPost]
        public async Task<ActionResult> GetAllPrograms()
        {
            List<ProgramUsageSummary> programs = await this.dashboardBase.GetAllProgramsUsages(this.User).ConfigureAwait(false);
            return this.Content(JsonConvert.SerializeObject(programs));
        }

        internal static Guid? GetTelemetryKeyFromUrl(string url)
        {
            if (url.IndexOf("telemetryKey=", StringComparison.OrdinalIgnoreCase) != -1)
            {
                string key = url.Substring(url.LastIndexOf("telemetryKey=", StringComparison.OrdinalIgnoreCase) + "telemetryKey=".Length);
                if (key.Contains("?"))
                {
                    key = key.Remove(key.IndexOf("?"));
                }
                if (Guid.TryParse(key, out Guid result))
                {
                    return result;
                }

            }
            return null;
        }

        internal static Guid? GetUpdatePackageGuidFromUrl(string url)
        {
            if (url.IndexOf("update-packages/", StringComparison.OrdinalIgnoreCase) != -1)
            {
                string key = url.Substring(url.LastIndexOf("update-packages/", StringComparison.OrdinalIgnoreCase) + "update-packages/".Length);
                if (key.Contains("/"))
                {
                    key = key.Remove(key.IndexOf("/"));
                }
                if (Guid.TryParse(key, out Guid result))
                {
                    return result;
                }

            }
            return null;
        }

        /// <summary>
        /// Gets all programs summary counts.
        /// </summary>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        public async Task<ActionResult> GetAllProgramsSummaryCounts()
        {
            AllProgramsSummaryData summary = await this.dashboardBase.GetAllProgramsSummaryCounts(this.User).ConfigureAwait(false);
            return this.PartialView("_AllProgramsSummaryBoxes", summary);
        }

        /// <summary>
        /// Gets the portal summary.
        /// </summary>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        public async Task<ActionResult> GetPortalSummary()
        {
            PortalSummaryData summary = await this.unitOfWork.GetPortalSummary().ConfigureAwait(false);
            return this.PartialView("_PortalSummaryBoxes", summary);
        }

        /// <summary>
        /// Portals this instance.
        /// </summary>
        /// <returns>ActionResult.</returns>
        public ActionResult Portal()
        {
            return this.View();
        }
    }
}