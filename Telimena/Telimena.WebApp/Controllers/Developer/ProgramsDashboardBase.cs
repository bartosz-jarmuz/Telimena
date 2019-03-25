using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Repository;

namespace Telimena.WebApp.Controllers.Developer
{
    /// <summary>
    /// Class ProgramsDashboardBase.
    /// </summary>
    public class ProgramsDashboardBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramsDashboardBase"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public ProgramsDashboardBase(IProgramsDashboardUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        /// The unit of work
        /// </summary>
        private readonly IProgramsDashboardUnitOfWork unitOfWork;

        /// <summary>
        /// Gets all programs.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task&lt;List&lt;ProgramSummary&gt;&gt;.</returns>
        public async Task<List<ProgramUsageSummary>> GetAllProgramsUsages(IPrincipal user)
        {
            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == user.Identity.Name);
            List<Program> programs = await this.unitOfWork.Programs.GetProgramsVisibleToUserAsync(telimenaUser, user).ConfigureAwait(false);
            List<ProgramUsageSummary> summary = (await this.unitOfWork.GetProgramUsagesSummary(programs).ConfigureAwait(false)).ToList();
            return summary;
        }

        /// <summary>
        /// Gets all programs.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task&lt;List&lt;ProgramSummary&gt;&gt;.</returns>
        public async Task<List<ProgramSummary>> GetAllProgramsSummary(IPrincipal user)
        {
            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == user.Identity.Name);
            List<Program> programs = await this.unitOfWork.Programs.GetProgramsVisibleToUserAsync(telimenaUser, user).ConfigureAwait(false);
            List<ProgramSummary> summary = (await this.unitOfWork.GetProgramSummary(programs).ConfigureAwait(false)).ToList();
            return summary;
        }

        /// <summary>
        /// Gets all programs.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task&lt;List&lt;ProgramSummary&gt;&gt;.</returns>
        public async Task<List<AppUsersSummaryData>> GetAppUsersSummary(IPrincipal user)
        {
            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == user.Identity.Name);
            List<Program> programs = await this.unitOfWork.Programs.GetProgramsVisibleToUserAsync(telimenaUser, user).ConfigureAwait(false);
            var summary = (await this.unitOfWork.GetAppUsersSummary(programs, null, null).ConfigureAwait(false)).ToList();
            return summary;
        }

        /// <summary>
        /// Gets all programs summary counts.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task&lt;AllProgramsSummaryData&gt;.</returns>
        public async Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts(IPrincipal user)
        {
            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == user.Identity.Name);
            List<Program> programs = await this.unitOfWork.Programs.GetProgramsVisibleToUserAsync(telimenaUser, user).ConfigureAwait(false);

            AllProgramsSummaryData summary = await this.unitOfWork.GetAllProgramsSummaryCounts(programs).ConfigureAwait(false);
            return summary;
        }
    }
}