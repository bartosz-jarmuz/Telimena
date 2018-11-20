using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.IO;
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
            try
            {
                if (!ApiRequestsValidator.IsRequestValid(request, out List<string> errors))
                {
                    throw new BadRequestException(string.Join(", ", errors));
                }

                if (await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == request.TelemetryKey) != null)
                {
                    throw new BadRequestException($"Use different telemetry key");
                }

                TelimenaUser user = await this.Work.Users.FirstOrDefaultAsync(x => x.UserName == this.User.Identity.Name);
                DeveloperAccount developerAccount = user.GetDeveloperAccountsLedByUser().FirstOrDefault();
                if (developerAccount == null)
                {
                    return new RegisterProgramResponse(new BadRequestException($"Cannot find developer account associated with user [{user.UserName}]"));
                }

                Program program = new Program(request.Name, request.TelemetryKey)
                {
                    Description = request.Description
                };
                developerAccount.AddProgram(program);

                var primaryAss = new ProgramAssembly()
                {
                    Name = Path.GetFileNameWithoutExtension(request.PrimaryAssemblyFileName), Extension = Path.GetExtension(request.PrimaryAssemblyFileName)
                };
                program.PrimaryAssembly = primaryAss;

                this.Work.Programs.Add(program);

                await this.Work.CompleteAsync();

                program = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == request.TelemetryKey);
                var url = Url?.Link("Default", new {Controller = "ProgramManagement", Action = "Index", telemetryKey = program.TelemetryKey});
                return new RegisterProgramResponse(program.TelemetryKey, program.DeveloperAccount.Id, url);
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException("Failed to register program. ", ex);
            }
        }

        [HttpDelete]
        [Audit]
        public async Task<IHttpActionResult> Delete(Guid telemetryKey)
        {
            var prg = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey);
            if (prg == null)
            {
                return this.BadRequest($"Program with key {telemetryKey} does not exist");
            }
            try
            {
                this.Work.Programs.Remove(prg);
                await this.Work.CompleteAsync();
            }
            catch (Exception ex)
            {
                return this.InternalServerError(new InvalidOperationException($"Error while deleting program {prg.Name} (Key: {telemetryKey})", ex));
            }
            return this.Ok($"Program {prg.Name} (Key: {telemetryKey}) deleted successfully");
        }

        [HttpPut]
        [Audit]
        public async Task<IHttpActionResult> SetUpdater(Guid telemetryKey, Guid updaterGuid)
        {
            Program prg = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == telemetryKey);
            if (prg == null)
            {
                return this.BadRequest($"Program with Key {telemetryKey} does not exist");
            }

            var updater = await this.Work.UpdaterRepository.GetUpdater(updaterGuid);
            if (updater == null)
            {
                return this.BadRequest($"Updater with Unique Id {updaterGuid} does not exist");
            }

            prg.Updater = updater;
            await this.Work.CompleteAsync();
            return this.Ok($"Updater set to {updater.InternalName}");
        }

        [HttpGet]
        public async Task<IEnumerable<Program>> GetPrograms(Guid developerGuid)
        {

            return await this.Work.Programs.GetAsync(x => x.DeveloperAccount.Guid  == developerGuid);
        }
    }
}