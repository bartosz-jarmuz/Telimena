using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Identity;
using ActionFilterAttribute = System.Web.Http.Filters.ActionFilterAttribute;

namespace MvcAuditLogger
{
    public class ApiAuditFilter : IActionFilter
    {
        public ApiAuditFilter()
        {
            this.logger = new DebugAuditLogger();
        }

        private readonly IAuditLogger logger;


        public bool AllowMultiple { get; } = false;

        public Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            AuditAttribute auditAttribute = actionContext.ActionDescriptor.GetCustomAttributes<AuditAttribute>().FirstOrDefault();
            if (auditAttribute == null)
            {
                return continuation();
            }

            AuditingLevel level = auditAttribute.Level;
            int levelModifier = 0;
            if (levelModifier >= 0) //-1 means audit disabled completely
            {

                Audit audit = new Audit()
                {
                    AuditID = Guid.NewGuid(),
                    SessionID = "",
                    UserName = (actionContext.RequestContext.Principal.Identity.IsAuthenticated) ? actionContext.RequestContext.Principal.Identity.Name : "Anonymous",
                    IPAddress = actionContext.Request.GetClientIp(),
                    AreaAccessed = actionContext.Request.RequestUri.ToString(),
                    Timestamp = DateTime.UtcNow,
                    Data = ""
                };
                AuditingModes mode = AuditingModes.DatabaseAndLogger;

                this.StoreAuditData(audit, mode, level);
            }

            return continuation();
        }

    

      
        private void StoreAuditData(Audit audit, AuditingModes mode, AuditingLevel level)
        {
            bool storeInFile = false;
            if (mode == AuditingModes.DatabaseAndLogger)
            {
                AuditingContext context = new AuditingContext();
                context.AuditRecords.Add(audit);
                context.SaveChanges();
                storeInFile = true;
            }
            else if (mode == AuditingModes.DatabaseOnly)
            {
                AuditingContext context = new AuditingContext();
                context.AuditRecords.Add(audit);
                context.SaveChanges();
            }
            else if (mode == AuditingModes.LoggerOnly)
            {
                storeInFile = true;
            }

            if (storeInFile)
            {
                if (level == AuditingLevel.BasicData)
                {
                    this.logger.StoreWithRequestData(audit);
                }
                else
                {
                    this.logger.StoreWithoutRequestData(audit);
                }
            }
        }

       
    }
}