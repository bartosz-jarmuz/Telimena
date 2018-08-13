using System.Web.Mvc;

namespace Telimena.WebApp.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult AccessDenied(string roles)
        {
            this.ViewBag.Roles = roles;
            this.Response.StatusCode = 403;
            return this.View();
        }
    }
}