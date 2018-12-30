using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;

namespace Telimena.WebApp.Controllers.Api.V1
{
    #region Using

    #endregion

    /// <summary>
    /// Developer (team) related controller
    /// </summary>
    [TelimenaApiAuthorize(Roles = TelimenaRoles.Developer)]
    [RoutePrefix("api/v{version:apiVersion}/developers")]
    public class DevelopersController : ApiController
    {
        /// <summary>
        /// New instance
        /// </summary>
        /// <param name="work"></param>
        public DevelopersController(IProgramsUnitOfWork work)
        {
            this.Work = work;
        }

        private IProgramsUnitOfWork Work { get; }

        ///// <summary>
        ///// Gets all programs for this developer team
        ///// </summary>
        ///// <param name="developerId"></param>
        ///// <returns></returns>
        //[HttpGet, Route("{developerId}/programs")]
        //public async Task<IEnumerable<Program>> GetPrograms(Guid developerId)
        //{
        //    return await this.Work.Programs.GetAsync(x => x.DeveloperAccount.Guid == developerId);
        //}

    }
}