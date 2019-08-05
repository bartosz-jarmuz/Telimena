using System;
using System.Collections.Generic;
using System.Web.Mvc;
using MvcAuditLogger;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Models.Shared;

namespace Telimena.WebApp.Controllers
{
    /// <summary>
    /// Class HomeController.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    [TelimenaAuthorize]
    public class HelpController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HelpController" /> class.
        /// </summary>
        public HelpController()
        {
        }


        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>ActionResult.</returns>
        [Audit]
        public ActionResult Index()
        {
            return this.View("Index");
        }

        /// <summary>
        /// Gettings the started.
        /// </summary>
        /// <returns>ActionResult.</returns>
        [Audit]
        public ActionResult GettingStarted()
        {
            return this.View("GettingStarted");
        }

        /// <summary>
        /// Telemetries this instance.
        /// </summary>
        /// <returns>ActionResult.</returns>
        [Audit]
        public ActionResult Telemetry()
        {
            return this.View("Telemetry");
        }

        /// <summary>
        /// Lifecycles the management.
        /// </summary>
        /// <returns>ActionResult.</returns>
        [Audit]
        public ActionResult LifecycleManagement()
        {
            return this.View("LifecycleManagement");
        }
    }
}