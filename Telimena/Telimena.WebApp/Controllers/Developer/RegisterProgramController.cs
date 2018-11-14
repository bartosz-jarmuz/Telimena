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
            this.Work = work;
            this.logger = logger;
        }

        private readonly IProgramsUnitOfWork Work;
        private ILog logger;

        [Audit]
        [HttpGet]
        public ActionResult Register()
        {
            return this.View();
        }

       
    }
}