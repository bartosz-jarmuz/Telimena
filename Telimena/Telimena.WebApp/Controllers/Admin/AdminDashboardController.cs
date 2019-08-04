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
using log4net;
using MvcAuditLogger;
using Newtonsoft.Json;
using Telimena.WebApp.Controllers.Developer;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Core.Models.Telemetry;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Models.AdminDashboard;

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
        /// <param name="logger">The logger.</param>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="auditingContext">The auditing context.</param>
        public AdminDashboardController(ILog logger, IProgramsDashboardUnitOfWork unitOfWork, AuditingContext auditingContext)
        {
            this.logger = logger;
            this.unitOfWork = unitOfWork;
            this.auditingContext = auditingContext;
            this.dashboardBase = new ProgramsDashboardBase(unitOfWork);
        }

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILog logger;
        /// <summary>
        /// The unit of work
        /// </summary>
        private readonly IProgramsDashboardUnitOfWork unitOfWork;
        /// <summary>
        /// The auditing context
        /// </summary>
        private readonly AuditingContext auditingContext;
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

        /// <summary>
        /// Gets the last audit data.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [HttpGet]
        public async Task<ActionResult> GetLastAuditData(IDataTablesRequest request)
        {
            List<Tuple<string, bool>> sorts = request.Columns.Where(x => x.Sort != null).OrderBy(x => x.Sort.Order).Select(x => new Tuple<string, bool>(x.Name, x.Sort.Direction == SortDirection.Descending)).ToList();

            string userName = this.User.Identity.Name;
            int totalCount = await this.auditingContext.AuditRecords.Where(x => x.UserName != userName).CountAsync().ConfigureAwait(false);
            IQueryable<Audit> query = this.auditingContext.AuditRecords.Where(x=>x.UserName != userName).AsQueryable();
            int take = request.Length;
            if (take == -1)
            {
                take = totalCount;
            }
            List<Audit> data = await query.OrderByMany(sorts).Skip(request.Start).Take(take).ToListAsync().ConfigureAwait(false);

            List<AuditViewModel> model = await this.ToViewModel(data).ConfigureAwait(false);


            DataTablesResponse response = DataTablesResponse.Create(request, totalCount, totalCount, model);

            return new DataTablesJsonResult(response, JsonRequestBehavior.AllowGet);

        }

        private async Task<List<AuditViewModel>> ToViewModel(List<Audit> data)
        {
            List<AuditViewModel> model = new List<AuditViewModel>();
            foreach (Audit audit in data)
            {
                AuditViewModel vm = AutoMapper.Mapper.Map<AuditViewModel>(audit);

                await this.AssignAppNameToAuditRecord(vm).ConfigureAwait(false);
                model.Add(vm);
            }
            return model;
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

        private async Task AssignAppNameToAuditRecord(AuditViewModel viewModel)
        {
            Guid? guid = GetTelemetryKeyFromUrl(viewModel.AreaAccessed);
            if (guid.HasValue)
            {
                string cacheKey = "AppName:" + guid.ToString();

                string name = (string)MemoryCache.Default.Get(cacheKey);
                if (string.IsNullOrEmpty(name))
                {
                    Program app = await this.unitOfWork.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == guid.Value).ConfigureAwait(false);
                    name = app.Name;
                    MemoryCache.Default.Add(cacheKey, name, new CacheItemPolicy() {AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1)});
                }

                viewModel.Application = name;
                return;
            }

            guid = GetUpdatePackageGuidFromUrl(viewModel.AreaAccessed);
            if (guid.HasValue)
            {
                string cacheKey = "AppName:" + guid.ToString();

                string name = (string)MemoryCache.Default.Get(cacheKey);
                if (string.IsNullOrEmpty(name))
                {
                    ProgramUpdatePackageInfo packageInfo = await this.unitOfWork.UpdatePackages.GetUpdatePackageInfo(guid.Value).ConfigureAwait(false);
                    if (packageInfo != null)
                    {
                        Program app = await this.unitOfWork.Programs.FirstOrDefaultAsync(x => x.Id == packageInfo.ProgramId).ConfigureAwait(false);
                        if (app != null)
                        {
                            name = app.Name;
                            MemoryCache.Default.Add(cacheKey, name, new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1) });
                        }
                    }
                }

                viewModel.Application = name;
            }
        }

        /// <summary>
        /// Applies the ordering query.
        /// </summary>
        /// <param name="sortBy">The sort by.</param>
        /// <param name="query">The query.</param>
        /// <returns>IOrderedQueryable&lt;Audit&gt;.</returns>
        private static IOrderedQueryable<Audit> ApplyOrderingQuery(IEnumerable<Tuple<string, bool>> sortBy, IQueryable<Audit> query) 
        {
            List<Tuple<string, bool>> rules = sortBy?.ToList();
            if (rules == null || !rules.Any())
            {
                rules = new List<Tuple<string, bool>>();
                {
                    new Tuple<string, bool>(nameof(TelemetryDetail.Id), true);
                };
            }

            foreach (Tuple<string, bool> rule in rules)
            {
                if (rule.Item1 == nameof(Audit.Timestamp) || rule.Item1 == nameof(TelemetryDetail.Timestamp) || rule.Item1 == nameof(TelemetryDetail.AssemblyVersion))
                {
                    query = query.OrderBy(rule.Item1, rule.Item2);
                }
            }

            IOrderedQueryable<Audit> orderedQuery = query as IOrderedQueryable<Audit>;
            return orderedQuery;
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
        [Audit]
        public ActionResult Portal()
        {
            return this.View();
        }
    }
}