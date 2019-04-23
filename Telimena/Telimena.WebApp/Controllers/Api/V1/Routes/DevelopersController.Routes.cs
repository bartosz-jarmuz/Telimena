using System;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;

namespace Telimena.WebApp.Controllers.Api.V1
{
    #region Using

    #endregion

    public partial class DevelopersController
    {
#pragma warning disable 1591
        public static class Routes
        {
            public const string GetPrograms = nameof(DevelopersController) + "." + nameof(DevelopersController.GetPrograms);
        }
    }

        
#pragma warning restore 1591
 }