using System.Web.Mvc;

namespace Telimena.WebApp.Controllers
{
    /// <summary>
    /// Class ErrorController.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    public class ErrorController : Controller
    {
        // GET: Error
        /// <summary>
        /// Accesses the denied.
        /// </summary>
        /// <param name="roles">The roles.</param>
        /// <returns>ActionResult.</returns>
        public ActionResult AccessDenied(string roles)
        {
            this.ViewBag.Roles = roles;
            this.Response.StatusCode = 403;
            return this.View();
        }
    }
}