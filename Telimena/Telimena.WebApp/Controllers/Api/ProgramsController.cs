using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Telimena.Client;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;

namespace Telimena.WebApp.Controllers.Api
{
    #region Using

    #endregion

    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
    public class ProgramsController : ApiController
    {
        public ProgramsController(IProgramsUnitOfWork work)
        {
            this.Work = work;
        }

        private IProgramsUnitOfWork Work { get; }

        [System.Web.Http.HttpGet]
        public async Task<IEnumerable<Program>> GetPrograms(int developerId)
        {
            return await this.Work.Programs.GetAsync(x => x.DeveloperAccount.Id == developerId);
        }

    }
}