using System.Linq;
using System.Web.Mvc;

namespace Telimena.WebApp.Controllers.Developer
{
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Models;
    using Infrastructure.Security;
    using Infrastructure.UnitOfWork;
    using log4net;
    using Models.Developer;
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class RegisterProgramController : Controller
    {
        private readonly IProgramsUnitOfWork _work;
        private ILog logger;
        public RegisterProgramController(IProgramsUnitOfWork work, ILog logger)
        {
            this._work = work;
            this.logger = logger;
        }

        [HttpGet]
        public ActionResult Register()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterProgramViewModel model)
        {
            Program prg = await this._work.Programs.FirstOrDefaultAsync(x => x.Name == model.ProgramName);
            if (prg == null)
            {
                this.ModelState.AddModelError("",$"Program [{model.ProgramName}] not found. Ensure it was used at least one time");
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