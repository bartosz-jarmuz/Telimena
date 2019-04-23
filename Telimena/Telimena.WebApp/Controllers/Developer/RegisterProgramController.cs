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
    /// <summary>
    /// Class RegisterProgramController.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class RegisterProgramController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterProgramController"/> class.
        /// </summary>
        /// <param name="work">The work.</param>
        /// <param name="logger">The logger.</param>
        public RegisterProgramController(IProgramsUnitOfWork work, ILog logger)
        {
            this.Work = work;
            this.logger = logger;
        }

        /// <summary>
        /// The work
        /// </summary>
        private readonly IProgramsUnitOfWork Work;
        /// <summary>
        /// The logger
        /// </summary>
        private ILog logger;

        /// <summary>
        /// Registers this instance.
        /// </summary>
        /// <returns>ActionResult.</returns>
        [Audit]
        [HttpGet]
        public ActionResult Register()
        {
            return this.View();
        }

       
    }
}