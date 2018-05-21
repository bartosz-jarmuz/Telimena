namespace Telimena.WebApi.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using WebApp.Core.Interfaces;
    using WebApp.Infrastructure.Security;

    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]

    public class ProgramDetailsController : Controller
    {
        public ProgramDetailsController()
        {
        }


        [HttpGet]
        public async Task<ActionResult> Index()
        {
            await Task.Delay(1);
            return this.View("Index");
        }


    }

    
}