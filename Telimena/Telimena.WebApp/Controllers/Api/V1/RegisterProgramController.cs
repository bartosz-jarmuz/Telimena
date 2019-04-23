using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using MvcAuditLogger;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Core.Models.Telemetry;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;

namespace Telimena.WebApp.Controllers.Api.V1
{
    /// <summary>
    /// Controls program management related endpoints
    /// </summary>
    [TelimenaApiAuthorize(Roles = TelimenaRoles.Developer)]
    [RoutePrefix("api/v{version:apiVersion}/register-program")]
    public partial class RegisterProgramController : ApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterProgramController"/> class.
        /// </summary>
        /// <param name="work">The work.</param>
        public RegisterProgramController(IRegisterProgramUnitOfWork work)
        {
            this.Work = work;
        }

        private IRegisterProgramUnitOfWork Work { get; }
        
        /// <summary>
        /// Register a new program in Telimena
        /// </summary>
        /// <param name = "request" ></param>
        /// <returns></returns>
        [Audit]
        [HttpPost, Route("", Name = Routes.Register)]
        public async Task<RegisterProgramResponse> Register(RegisterProgramRequest request)
        {
            try
            {
                if (!ApiRequestsValidator.IsRequestValid(request, out List<string> errors))
                {
                    return new RegisterProgramResponse(new BadRequestException(string.Join(", ", errors)));
                }

                if (await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == request.TelemetryKey).ConfigureAwait(false) != null)
                {
                    return new RegisterProgramResponse(new BadRequestException($"Use different telemetry key"));
                }

                TelimenaUser user = await this.Work.Users.FirstOrDefaultAsync(x => x.UserName == this.User.Identity.Name).ConfigureAwait(false);
                DeveloperTeam developerTeam = user.GetDeveloperAccountsLedByUser().FirstOrDefault();
                if (developerTeam == null)
                {
                    return new RegisterProgramResponse(new BadRequestException($"Cannot find developer account associated with user [{user.UserName}]"));
                }

                Program program = new Program(request.Name, request.TelemetryKey)
                {
                    Description = request.Description
                };

                ProgramAssembly primaryAss = new ProgramAssembly()
                {
                    Name = Path.GetFileNameWithoutExtension(request.PrimaryAssemblyFileName),
                    Extension = Path.GetExtension(request.PrimaryAssemblyFileName)
                };

                await this.Work.RegisterProgram(developerTeam, program, primaryAss).ConfigureAwait(false);

                program = await this.Work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == request.TelemetryKey).ConfigureAwait(false);
                var url = this.Url?.Link("Default", new { Controller = "ProgramManagement", Action = "Index", telemetryKey = program.TelemetryKey });
                return new RegisterProgramResponse(program.TelemetryKey, program.DeveloperTeam.PublicId, url);
            }
            catch (Exception ex)
            {
                return new RegisterProgramResponse(ex);
            }
        }
    }
}