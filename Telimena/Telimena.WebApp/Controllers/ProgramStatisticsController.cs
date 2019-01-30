using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Web.Mvc;
using DataTables.AspNet.Core;
using DataTables.AspNet.Mvc5;
using MvcAuditLogger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telimena.WebApp.Core;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Models.ProgramStatistics;

namespace Telimena.WebApp.Controllers
{
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class ProgramStatisticsController : Controller
    {
        public ProgramStatisticsController(IProgramsDashboardUnitOfWork work)
        {
            this.Work = work;
        }

        private IProgramsDashboardUnitOfWork Work { get; }

        [Audit]
        [HttpGet]
        public async Task<ActionResult> Index(Guid telemetryKey)
        {
            Program program = await this.Work.Programs.SingleOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            ProgramStatisticsViewModel model = new ProgramStatisticsViewModel() {TelemetryKey = program.TelemetryKey, ProgramName = program.Name};

            return this.View("Index", model);
        }

        [Audit]
        [HttpGet]
        public async Task<ActionResult> PivotTable(Guid telemetryKey)
        {
            Program program = await this.Work.Programs.SingleOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            ProgramStatisticsViewModel model = new ProgramStatisticsViewModel() { TelemetryKey = program.TelemetryKey, ProgramName = program.Name };

            return this.View("PivotTable", model);
        }

     
        [HttpGet]
        public async Task<JsonResult> GetPivotTableData(TelemetryItemTypes type, Guid telemetryKey)
        {
            var result = await this.Work.GetPivotTableData(type, telemetryKey).ConfigureAwait(false);
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public async Task<JsonResult> GetProgramViewsUsageData(Guid telemetryKey, IDataTablesRequest request)
        {
            IEnumerable<Tuple<string, bool>> sorts = request.Columns.Where(x => x.Sort != null).OrderBy(x => x.Sort.Order).Select(x => new Tuple<string, bool>(x.Name, x.Sort.Direction == SortDirection.Descending));

            UsageDataTableResult result = await this.Work.GetProgramViewsUsageData(telemetryKey, request.Start, request.Length, sorts).ConfigureAwait(false);

            DataTablesResponse response = DataTablesResponse.Create(request, result.TotalCount, result.FilteredCount, result.UsageData);

            return new DataTablesJsonResult(response, JsonRequestBehavior.AllowGet);
        }

    }
}