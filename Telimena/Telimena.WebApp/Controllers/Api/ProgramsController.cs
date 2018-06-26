namespace Telimena.WebApi.Controllers
{
    #region Using
    using System;
    using System.IdentityModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Client;
    using WebApp.Core.Interfaces;
    using WebApp.Core.Messages;
    using WebApp.Core.Models;
    using WebApp.Infrastructure;
    using WebApp.Infrastructure.Security;
    using WebApp.Infrastructure.UnitOfWork;
    #endregion

    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class ProgramsController : ApiController
    {
        public ProgramsController(IProgramsUnitOfWork work)
        {
            this.work = work;
        }

        private readonly IProgramsUnitOfWork work;

        [HttpPost]
        public async Task<IHttpActionResult> SetLatestVersion(SetLatestVersionRequest request)
        {
            if (!ApiRequestsValidator.IsRequestValid(request))
            {
                return this.BadRequest($"SetLatestVersionRequest is invalid");
            }

            var prg = await this.work.Programs.FirstOrDefaultAsync(x => x.ProgramId == request.ProgramId);
            if (prg == null)
            {
                return this.BadRequest($"Program [{request.ProgramId}] not found");
            }

            if (!Version.TryParse(request.Version, out Version _))
            {
                return this.BadRequest($"Version [{request.Version}] is not in valid format. Expected e.g. 1.0.0.0");
            }

            var assVersion = prg.PrimaryAssembly.GetVersion(request.Version);
            if (assVersion == null)
            {
                assVersion = new AssemblyVersion()
                {
                    ProductionReleaseDate = DateTime.UtcNow,
                    Version = request.Version
                };
            }

            prg.PrimaryAssembly.LatestVersion = assVersion;

            return this.Ok();
        }
        
        [AllowAnonymous]
        [HttpGet]
        public async Task<IHttpActionResult> GetLatestVersion(int programId)
        {
            var prg = await this.work.Programs.FirstOrDefaultAsync(x => x.ProgramId == programId);
            if (prg == null)
            {
                return this.BadRequest($"Program [{programId}] not found");
            }

            return this.Ok(prg.PrimaryAssembly.LatestVersion.Version);
        }



    }
}