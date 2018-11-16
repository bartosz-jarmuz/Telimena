using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using MvcAuditLogger;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;

namespace Telimena.WebApp.Controllers.Api
{
    #region Using

    #endregion

    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class ProgramsController : ApiController
    {
        public ProgramsController(IProgramsUnitOfWork work)
        {
            this.Work = work;
        }

        private IProgramsUnitOfWork Work { get; }

        [HttpPost]
        [Audit]
        public async Task<RegisterProgramResponse> Register(RegisterProgramRequest request)
        {
            if (!ApiRequestsValidator.IsRequestValid(request, out List<string> errors))
            {
                return new RegisterProgramResponse(new BadRequestException(string.Join(", ", errors)));
            }

            try
            {
                TelimenaUser user = await this.Work.Users.FirstOrDefaultAsync(x => x.UserName == this.User.Identity.Name);
                DeveloperAccount developerAccount = user.GetDeveloperAccountsLedByUser().FirstOrDefault();
                if (developerAccount == null)
                {
                    return new RegisterProgramResponse(new BadRequestException($"Cannot find developer account associated with user [{user.UserName}]"));
                }

                Program program = new Program(request.Name)
                {
                    Description = request.Description
                };
                Guid guid = program.TelemetryKey;
                developerAccount.AddProgram(program);

                this.Work.Programs.Add(program);

                await this.Work.CompleteAsync();

                program = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == guid);
                var url = Url.Link("Default", new {Controller = "ProgramManagement", Action = "Index", telemetryKey = program.TelemetryKey});
                return new RegisterProgramResponse(program.Id, program.TelemetryKey, program.DeveloperAccount.Id, url);
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException("Failed to register program. ", ex);
            }
        }

        [HttpDelete]
        [Audit]
        public async Task<IHttpActionResult> Delete(int id)
        {
            var prg = await this.Work.Programs.FirstOrDefaultAsync(x => x.Id == id);
            if (prg == null)
            {
                return this.BadRequest($"Program with Id {id} does not exist");
            }
            try
            {
                this.Work.Programs.Remove(prg);
                await this.Work.CompleteAsync();
            }
            catch (Exception ex)
            {
                return this.InternalServerError(new InvalidOperationException($"Error while deleting program {prg.Name} (ID: {id})", ex));
            }
            return this.Ok($"Program {prg.Name} (ID: {id}) deleted successfully");
        }

        [HttpPut]
        [Audit]
        public async Task<IHttpActionResult> SetUpdater(int programId, int updaterId)
        {
            var prg = await this.Work.Programs.FirstOrDefaultAsync(x => x.Id == programId);
            if (prg == null)
            {
                return this.BadRequest($"Program with Id {programId} does not exist");
            }

            var updater = await this.Work.UpdaterRepository.GetUpdater(updaterId);
            if (updater == null)
            {
                return this.BadRequest($"Updater with Id {updaterId} does not exist");
            }

            prg.Updater = updater;
            await this.Work.CompleteAsync();
            return this.Ok($"Updater set to {updater.InternalName}");
        }

        [HttpGet]
        public async Task<IEnumerable<Program>> GetPrograms(int developerId)
        {
            return await this.Work.Programs.GetAsync(x => x.DeveloperAccount.Id == developerId);
        }
    }
}