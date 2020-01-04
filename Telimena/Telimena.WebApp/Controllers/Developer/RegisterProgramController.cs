using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

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
        public RegisterProgramController()
        {
        } 

        /// <summary>
        /// Registers this instance.
        /// </summary>
        /// <returns>ActionResult.</returns>
        
        [HttpGet]
        public ActionResult Register()
        {
            return this.View();
        }

       
    }
}