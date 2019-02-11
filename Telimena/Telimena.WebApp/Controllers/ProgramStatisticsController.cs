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
    /// <summary>
    /// Class ProgramStatisticsController.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class ProgramStatisticsController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramStatisticsController" /> class.
        /// </summary>
        /// <param name="work">The work.</param>
        public ProgramStatisticsController(IProgramsDashboardUnitOfWork work)
        {
            this.Work = work;
        }

        /// <summary>
        /// Gets the work.
        /// </summary>
        /// <value>The work.</value>
        private IProgramsDashboardUnitOfWork Work { get; }

        /// <summary>
        /// Indexes the specified telemetry key.
        /// </summary>
        /// <param name="telemetryKey">The telemetry key.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
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

        /// <summary>
        /// Pivots the table.
        /// </summary>
        /// <param name="telemetryKey">The telemetry key.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
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

        [Audit]
        [HttpGet]
        public async Task<ActionResult> SequenceHistory(Guid telemetryKey, string sequenceId)
        {
            Program program = await this.Work.Programs.SingleOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            SequenceHistoryViewModel model = new SequenceHistoryViewModel()
            {
                TelemetryKey = program.TelemetryKey, ProgramName = program.Name, SequenceId = sequenceId
            };

            return this.View("SequenceHistory", model);
        }

        /// <summary>
        /// Shows exceptions view
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>
        [Audit]
        [HttpGet]
        public async Task<ActionResult> Exceptions(Guid telemetryKey)
        {
            Program program = await this.Work.Programs.SingleOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            ProgramStatisticsViewModel model = new ProgramStatisticsViewModel() { TelemetryKey = program.TelemetryKey, ProgramName = program.Name };

            return this.View("Exceptions", model);
        }



        /// <summary>
        /// Shows exceptions view
        /// </summary>
        /// <param name="telemetryKey"></param>
        /// <returns></returns>
        [Audit]
        [HttpGet]
        public async Task<ActionResult> Logs(Guid telemetryKey)
        {
            Program program = await this.Work.Programs.SingleOrDefaultAsync(x => x.TelemetryKey == telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            ProgramStatisticsViewModel model = new ProgramStatisticsViewModel() { TelemetryKey = program.TelemetryKey, ProgramName = program.Name };

            return this.View("Logs", model);
        }


        /// <summary>
        /// Gets the pivot table data.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="telemetryKey">The telemetry key.</param>
        /// <returns>Task&lt;JsonResult&gt;.</returns>
        [HttpGet]
        public async Task<JsonResult> GetPivotTableData(TelemetryItemTypes type, Guid telemetryKey)
        {
            var result = await this.Work.GetPivotTableData(type, telemetryKey).ConfigureAwait(false);
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the program views usage data.
        /// </summary>
        /// <param name="telemetryKey">The telemetry key.</param>
        /// <param name="itemType"></param>
        /// <param name="request">The request.</param>
        /// <returns>Task&lt;JsonResult&gt;.</returns>
        [HttpGet]
        public async Task<JsonResult> GetProgramUsageData(Guid telemetryKey, TelemetryItemTypes itemType, IDataTablesRequest request)
        {
            IEnumerable<Tuple<string, bool>> sorts = request.Columns.Where(x => x.Sort != null).OrderBy(x => 
                x.Sort.Order).Select(x => new Tuple<string, bool>(x.Name, x.Sort.Direction == SortDirection.Descending));
            UsageDataTableResult result;

            var searchableColumns = request.Columns.Where(x => x.IsSearchable).Select(x => x.Name).ToList();

            if (itemType == TelemetryItemTypes.Exception)
            {
                result = await this.Work.GetExceptions(telemetryKey, itemType, request.Start, request.Length, sorts, request.Search, searchableColumns).ConfigureAwait(false);
            }
            else if (itemType == TelemetryItemTypes.LogMessage)
            {
                result = await this.Work.GetLogs(telemetryKey,  request.Start, request.Length, sorts, request.Search.Value).ConfigureAwait(false);
            }
            else
            {
                result= await this.Work.GetProgramViewsUsageData(telemetryKey, itemType ,request.Start, request.Length, sorts, request.Search, searchableColumns).ConfigureAwait(false);
            }

            DataTablesResponse response = DataTablesResponse.Create(request, result.TotalCount, result.FilteredCount, result.UsageData);

            return new DataTablesJsonResult(response, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// Gets the sequence history.
        /// </summary>
        /// <param name="telemetryKey">The telemetry key.</param>
        /// <param name="sequenceId">The sequence identifier.</param>
        /// <param name="request">The request.</param>
        /// <returns>Task&lt;JsonResult&gt;.</returns>
        [HttpGet]
        public async Task<JsonResult> GetSequenceHistoryData(Guid telemetryKey, string sequenceId, IDataTablesRequest request)
        {
            IEnumerable<Tuple<string, bool>> sorts = request.Columns.Where(x => x.Sort != null).OrderBy(x =>
                x.Sort.Order).Select(x => new Tuple<string, bool>(x.Name, x.Sort.Direction == SortDirection.Descending));
            UsageDataTableResult result = await this.Work.GetSequenceHistory(telemetryKey, sequenceId, request.Search.Value).ConfigureAwait(false);

            DataTablesResponse response = DataTablesResponse.Create(request, result.TotalCount, result.FilteredCount, result.UsageData);

            return new DataTablesJsonResult(response, JsonRequestBehavior.AllowGet);
        }
    }
}