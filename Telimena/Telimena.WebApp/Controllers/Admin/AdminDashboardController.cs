using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Security;

namespace Telimena.WebApp.Controllers.Admin
{
    [TelimenaAuthorize(Roles = TelimenaRoles.Admin)]
    public class AdminDashboardController : Controller
    {
        public AdminDashboardController(ILog logger, IProgramsDashboardUnitOfWork unitOfWork, AuditingContext auditingContext)
        {
            this.logger = logger;
            this.unitOfWork = unitOfWork;
            this.auditingContext = auditingContext;
            this.dashboardBase = new ProgramsDashboardBase(unitOfWork);
        }

        private readonly ILog logger;
        private readonly IProgramsDashboardUnitOfWork unitOfWork;
        private readonly AuditingContext auditingContext;
        private readonly ProgramsDashboardBase dashboardBase;

        [Audit]
        public ActionResult Apps()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<ActionResult> GetAllPrograms()
        {
            List<ProgramSummary> programs = await this.dashboardBase.GetAllPrograms(this.User);
            return this.Content(JsonConvert.SerializeObject(programs));
        }

        [HttpGet]
        public async Task<ActionResult> GetLastAuditData(IDataTablesRequest request)
        {
            List<Tuple<string, bool>> sorts = request.Columns.Where(x => x.Sort != null).OrderBy(x => x.Sort.Order).Select(x => new Tuple<string, bool>(x.Name, x.Sort.Direction == SortDirection.Descending)).ToList();


            var totalCount = await this.auditingContext.AuditRecords.CountAsync();
            var query = this.auditingContext.AuditRecords.AsQueryable();
            var take = request.Length;
            if (take == -1)
            {
                take = totalCount;
            }
            List<Audit> data = await query.OrderByMany(sorts).Skip(request.Start).Take(take).ToListAsync();

            DataTablesResponse response = DataTablesResponse.Create(request, totalCount, totalCount, data);

            return new DataTablesJsonResult(response, JsonRequestBehavior.AllowGet);

        }

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

            var orderedQuery = query as IOrderedQueryable<Audit>;
            return orderedQuery;
        }



        public async Task<ActionResult> GetAllProgramsSummaryCounts()
        {
            AllProgramsSummaryData summary = await this.dashboardBase.GetAllProgramsSummaryCounts(this.User);
            return this.PartialView("_AllProgramsSummaryBoxes", summary);
        }

        public async Task<ActionResult> GetPortalSummary()
        {
            PortalSummaryData summary = await this.unitOfWork.GetPortalSummary();
            return this.PartialView("_PortalSummaryBoxes", summary);
        }

        [Audit]
        public ActionResult Portal()
        {
            return this.View();
        }
    }
}