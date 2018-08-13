using System.Web.Mvc;

namespace Telimena.WebApp.Controllers
{
    using Core.Interfaces;
    using System.Threading.Tasks;
    using Infrastructure.Security;

    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class DeveloperDashboardController : Controller
    {
        public ActionResult Index()
        {
            return this.View();
        }

        public Task<ActionResult> DeveloperSummary()
        {
            return null;
        }


    }
}
