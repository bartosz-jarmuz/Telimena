using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using log4net;
using MvcAuditLogger;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Models.Developer;

namespace Telimena.WebApp.Controllers.Developer
{
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class RegisterProgramController : Controller
    {
        public RegisterProgramController(IProgramsUnitOfWork work, ILog logger)
        {
            this._work = work;
            this.logger = logger;
        }

        private readonly IProgramsUnitOfWork _work;
        private ILog logger;

        [Audit]
        [HttpGet]
        public ActionResult Register()
        {
            return this.View();
        }

        [Audit]
        [HttpPost]
        public async Task<ActionResult> Register(RegisterProgramViewModel model)
        {
            Program prg = await this._work.Programs.FirstOrDefaultAsync(x => x.Name == model.ProgramName);
            if (prg == null)
            {
                this.ModelState.AddModelError("", $"Program [{model.ProgramName}] not found. Ensure it was used at least one time");
                return this.View(model);
            }

            TelimenaUser user = await this._work.Users.FirstOrDefaultAsync(x => x.UserName == this.User.Identity.Name);
            DeveloperAccount dev = user.GetDeveloperAccountsLedByUser().FirstOrDefault();
            if (dev == null)
            {
                this.ModelState.AddModelError("", $"Cannot find developer account associated with user [{user.UserName}]");
                return this.View(model);
            }

            if (prg.DeveloperAccount != null)
            {
                this.ModelState.AddModelError("", $"Program [{model.ProgramName}] is already registered with another developer: [{prg.DeveloperAccount.Name}]");
                return this.View(model);
            }


            dev.AddProgram(prg);

            await this._work.CompleteAsync();
            model.IsSuccess = true;
            return this.View(model);
        }
    }
}