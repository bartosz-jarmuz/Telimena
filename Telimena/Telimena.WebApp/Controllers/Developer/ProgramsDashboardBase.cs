using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Repository;

namespace Telimena.WebApp.Controllers.Developer
{
    public class ProgramsDashboardBase
    {
        public ProgramsDashboardBase(IProgramsDashboardUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        private readonly IProgramsDashboardUnitOfWork unitOfWork;

        public async Task<List<ProgramSummary>> GetAllPrograms(IPrincipal user)
        {
            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == user.Identity.Name);
            List<Program> programs = await this.unitOfWork.Programs.GetProgramsVisibleToUserAsync(telimenaUser, user).ConfigureAwait(false);
            List<ProgramSummary> summary = (await this.unitOfWork.GetProgramsSummary(programs).ConfigureAwait(false)).ToList();
            return summary;
        }

        public async Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts(IPrincipal user)
        {
            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == user.Identity.Name);
            List<Program> programs = await this.unitOfWork.Programs.GetProgramsVisibleToUserAsync(telimenaUser, user).ConfigureAwait(false);

            AllProgramsSummaryData summary = await this.unitOfWork.GetAllProgramsSummaryCounts(programs).ConfigureAwait(false);
            return summary;
        }
    }
}