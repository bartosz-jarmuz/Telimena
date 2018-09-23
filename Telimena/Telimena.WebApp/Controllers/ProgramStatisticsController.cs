using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using DataTables.AspNet.Core;
using DataTables.AspNet.Mvc5;
using Newtonsoft.Json;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.DataTables;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Models.ProgramDetails;
using Telimena.WebApp.Models.ProgramStatistics;

namespace Telimena.WebApi.Controllers
{
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class ProgramStatisticsController : Controller
    {
        public ProgramStatisticsController(IProgramsDashboardUnitOfWork work)
        {
            this.Work = work;
        }

        private IProgramsDashboardUnitOfWork Work { get; }

        [HttpGet]
        public async Task<ActionResult> Index(string programName)
        {
            if (string.IsNullOrEmpty(programName))
            {
                return this.RedirectToAction("Index", "Home");
            }

            Program program = await this.Work.Programs.FirstOrDefaultAsync(x => x.Name == programName);

            ProgramStatisticsViewModel model = new ProgramStatisticsViewModel() {ProgramId = program.Id, ProgramName = program.Name};

            return this.View("Index", model);
        }
  

        public async Task<JsonResult> GetProgramUsageData(int programId, IDataTablesRequest request)
        {
            UsageDataTableResult result = await this.Work.GetProgramUsageData(programId, request.Start, request.Length, "Id",true);

            DataTablesResponse response = DataTablesResponse.Create(request, result.TotalCount, result.FilteredCount, result.UsageData);

            return new DataTablesJsonResult(response, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetProgramFunctionsUsageData(int programId, IDataTablesRequest request)
        {
            UsageDataTableResult result = await this.Work.GetProgramFunctionsUsageData(programId, request.Start, request.Length, "Id", true);

            DataTablesResponse response = DataTablesResponse.Create(request, result.TotalCount, result.FilteredCount, result.UsageData);

            return new DataTablesJsonResult(response, JsonRequestBehavior.AllowGet);
        }

    }
}