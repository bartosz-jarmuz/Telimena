using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ProgramUpdatesController : ApiController
    {
        public ProgramUpdatesController(IProgramsUnitOfWork work)
        {
            this.Work = work;
        }

        private IProgramsUnitOfWork Work { get; }

        [HttpGet]
        public IHttpActionResult ValidatePackageVersion(CreateUpdatePackageRequest request)
        {
            if (!ApiRequestsValidator.IsRequestValid(request, out List<string> errors))
            {
                return this.BadRequest(string.Join(", ", errors));
            }

            return this.Ok();
        }

        [HttpPost]
        public async Task<IHttpActionResult> UploadUpdatePackage()
        {
            try
            {
                string reqString = HttpContext.Current.Request.Form["Model"];
                CreateUpdatePackageRequest request = JsonConvert.DeserializeObject<CreateUpdatePackageRequest>(reqString);
                HttpPostedFile uploadedFile = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    UpdatePackage pkg = await this.Work.UpdatePackages.StorePackageAsync(request.ProgramId, request.PackageVersion, uploadedFile.InputStream,
                        uploadedFile.FileName);
                    await this.Work.CompleteAsync();
                    return this.Ok(pkg.Id);
                }

                return this.BadRequest("Empty attachment");
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }
 
    }
}