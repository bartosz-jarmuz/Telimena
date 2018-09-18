using System.Threading.Tasks;
using System.Web.Mvc;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Infrastructure.Security;

namespace Telimena.WebApp.Controllers
{
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class DeveloperDashboardController : Controller
    {
        public Task<ActionResult> DeveloperSummary()
        {
            return null;
        }

        public ActionResult Index()
        {
            return this.View();
        }
    }
}