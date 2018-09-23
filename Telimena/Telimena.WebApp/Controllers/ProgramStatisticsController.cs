using System;
using System.IdentityModel;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Web.Mvc;
using DataTables.AspNet.Core;
using DataTables.AspNet.Mvc5;
using Newtonsoft.Json;
using Telimena.WebApp.Core.DTO;
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

        [HttpGet]
        public async Task<ActionResult> ExportFunctionUsageCustomData(int programId, bool includeGenericData)
        {
            Program program = await this.Work.Programs.FirstOrDefaultAsync(x => x.Id == programId);
            if (program == null)
            {
                throw new BadRequestException($"Program {programId} does not exist");
            }

            dynamic obj = await this.Work.ExportFunctionsUsageCustomData(programId, includeGenericData);
            //obj.programId = programId;
            //obj.programName = program.Name;
            string content = JsonConvert.SerializeObject(obj);
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            FileContentResult result = new FileContentResult(bytes, "text/plain");
            result.FileDownloadName = $"{DateTime.UtcNow:yyyy-MM-dd HH-mm-ss}_FunctionsCustomDataExport_{program.Name}.json";
            return result;
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