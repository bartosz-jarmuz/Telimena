using System;
using System.Collections.Generic;
using System.Data;
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
using Telimena.WebApp.Core.Models.Portal;
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
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            ProgramStatisticsViewModel model = new ProgramStatisticsViewModel() {TelemetryKey = program.TelemetryKey, ProgramName = program.Name};
            //model.EventNames = (await this.Work.GetEventNames(program).ConfigureAwait(false));
            //model.ViewsNames = await this.Work.GetViewNames(program).ConfigureAwait(false);
            return this.View("Index", model);
        }

        /// <summary>
        /// Indexes the specified telemetry key.
        /// </summary>
        /// <param name="telemetryKey">The telemetry key.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [Audit]
        [HttpGet]
        public async Task<ActionResult> Dashboard(Guid telemetryKey)
        {
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            ProgramStatisticsViewModel model = new ProgramStatisticsViewModel() { TelemetryKey = program.TelemetryKey, ProgramName = program.Name };

            return this.View("Dashboard", model);
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
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);

            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            ProgramStatisticsViewModel model = new ProgramStatisticsViewModel() { TelemetryKey = program.TelemetryKey, ProgramName = program.Name };

            return this.View("PivotTable", model);
        }


        /// <summary>
        /// Sequences the history.
        /// </summary>
        /// <param name="telemetryKey">The telemetry key.</param>
        /// <param name="sequenceId">The sequence identifier.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [Audit]
        [HttpGet]
        public async Task<ActionResult> SequenceHistory(Guid telemetryKey, string sequenceId)
        {
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);

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
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);

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
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);

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
                result= await this.Work.GetProgramUsageData(telemetryKey, itemType ,request.Start, request.Length, sorts, request.Search, searchableColumns).ConfigureAwait(false);
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


        /// <summary>
        /// Gets the application users summary.
        /// </summary>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [HttpPost]
        public async Task<ActionResult> GetAppUsersSummary(Guid telemetryKey, DateTime startDate, DateTime endDate)
        {
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            List<AppUsersSummaryData> programs = await this.Work.GetAppUsersSummary(new List<Program>(){program}, startDate,endDate).ConfigureAwait(false);
            return this.Content(JsonConvert.SerializeObject(programs));
        }

        /// <summary>
        /// Gets the program usages.
        /// </summary>
        /// <param name="telemetryKey">The telemetry key.</param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [HttpPost]
        public async Task<ActionResult> GetProgramUsages(Guid telemetryKey)
        {
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            List<ProgramUsageSummary> programs = (await this.Work.GetProgramUsagesSummary(new List<Program>() { program }).ConfigureAwait(false)).ToList();
            return this.Content(JsonConvert.SerializeObject(programs));
        }

        /// <summary>
        /// Gets all programs summary counts.
        /// </summary>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        public async Task<ActionResult> GetSummaryCounts(Guid telemetryKey)
        {
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            AllProgramsSummaryData summary = await this.Work.GetAllProgramsSummaryCounts(new List<Program>() { program }).ConfigureAwait(false);
            return this.PartialView("_AllProgramsSummaryBoxes", summary);
        }

        [HttpGet]
        public async Task<ActionResult> GetDailyUsages(Guid telemetryKey, DateTime startDate, DateTime endDate)
        {
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            var dt = await this.Work.GetDailyActivityScore(program, startDate, endDate).ConfigureAwait(false);

            List<object> iData = new List<object>();
            foreach (DataColumn dc in dt.Columns)
            {
                List<object> x = new List<object>();
                x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
                iData.Add(x);
            }

            return this.Json(iData, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetDailyUsersCount(Guid telemetryKey, DateTime startDate, DateTime endDate)
        {
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            var dt = await this.Work.GetDailyUsersCount(program , startDate, endDate).ConfigureAwait(false);

            List<object> iData = new List<object>();
            foreach (DataColumn dc in dt.Columns)
            {
                List<object> x = new List<object>();
                x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
                iData.Add(x);
            }

            return this.Json(iData, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the version distribution.
        /// </summary>
        /// <param name="telemetryKey">The telemetry key.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [HttpGet]
        public async Task<ActionResult> GetVersionDistribution(Guid telemetryKey, DateTime startDate, DateTime endDate)
        {
            Program program = await this.Work.Programs.GetByTelemetryKey(telemetryKey).ConfigureAwait(false);
            if (program == null)
            {
                return this.RedirectToAction("Index", "Home");
            }

            var dt = await this.Work.GetVersionDistribution(program, startDate, endDate).ConfigureAwait(false);

            List<object> iData = new List<object>();
            foreach (DataColumn dc in dt.Columns)
            {
                List<object> x = new List<object>();
                x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
                iData.Add(x);
            }

            return this.Json(iData, JsonRequestBehavior.AllowGet);
        }

    }
}