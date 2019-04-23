using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Telimena.WebApp.Infrastructure;

namespace MvcAuditLogger
{
    #region Using

    #endregion

    public class AuditFilter : IActionFilter
    {


        public AuditFilter(IAuditLogger logger)
        {
            this.logger = logger;
        }

        private readonly IAuditLogger logger;

        /// <summary>Called after the action method executes.</summary>
        /// <param name="filterContext">The filter context.</param>
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        /// <summary>
        ///     Called by the ASP.NET MVC framework before the action method executes.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            AuditAttribute auditAttribute = filterContext.ActionDescriptor.GetCustomAttributes(true).OfType<AuditAttribute>().FirstOrDefault();
            if (auditAttribute == null)
            {
                return;
            }

            AuditingLevel level = auditAttribute.Level;
            int levelModifier = 0;
            //int levelModifier = this.config.Get<int>(ConfigKeys.Portal.AuditingLevelModifier, 0);
            if (levelModifier >= 0) //-1 means audit disabled completely
            {
                HttpRequestBase request = filterContext.HttpContext.Request;
                Audit audit = new Audit()
                {
                    AuditID = Guid.NewGuid(),
                    SessionID = this.GetSessionId(request),
                    UserName = (request.IsAuthenticated) ? filterContext.HttpContext.User.Identity.Name : "Anonymous",
                    IPAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress,
                    AreaAccessed = request.RawUrl,
                    Timestamp = DateTime.UtcNow,
                    Data = this.SerializeRequest(request, (uint) levelModifier, level)
                };
                AuditingModes mode = AuditingModes.DatabaseAndLogger;

                this.StoreAuditData(audit, mode, level);
            }
        }

        private string GetSessionId(HttpRequestBase request)
        {
            string cookie = request.Cookies[FormsAuthentication.FormsCookieName]?.Value ?? "Anonymous";
            byte[] bytes = Encoding.ASCII.GetBytes(cookie);
            string id = string.Join("", MD5.Create().ComputeHash(bytes).Select(s => s.ToString("x2")));
            return id;
        }

        /// <summary>
        ///     Serializes the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="levelModifier"></param>
        /// <returns>System.String.</returns>
        private string SerializeRequest(HttpRequestBase request, uint levelModifier, AuditingLevel level)
        {
            level = IncreaseEnumValue(level, levelModifier);

            switch (level)
            {
                case AuditingLevel.NoData:
                default:
                    return "";
                //case AuditingLevel.BasicData:
                //    return JsonConvertWrapper.SerializeObject(new {request.Cookies, request.Headers, request.QueryString, request.UrlReferrer});
                //case AuditingLevel.AdvancedData:
                //    return JsonConvertWrapper.SerializeObject(new
                //    {
                //        request.Cookies,
                //        request.Headers,
                //        request.QueryString,
                //        request.UrlReferrer,
                //        request.Files,
                //        request.Form,
                //        request.Params,
                //        request.Browser,
                //        request.LogonUserIdentity,
                //    });
            }
        }

        private static T IncreaseEnumValue<T>(T theEnum, uint modifier) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException($"The requested type {typeof(T).Name} is not an enum!");
            }

            int value = (Convert.ToInt32(theEnum) + (int)modifier);

            T last = Enum.GetValues(typeof(T)).Cast<T>().Max();
            int lastValue = Convert.ToInt32(last);
            while (!Enum.IsDefined(typeof(T), value))
            {
                if (modifier == 0)
                {
                    return theEnum;
                }
                if (value >= lastValue)
                {
                    return last;
                }

                modifier--;
                value = (Convert.ToInt32(theEnum) + (int)modifier);
            }

            return (T)Enum.Parse(typeof(T), value.ToString());
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