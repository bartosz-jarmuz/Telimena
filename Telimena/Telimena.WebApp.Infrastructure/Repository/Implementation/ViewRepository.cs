
using System;
using System.Data.Entity;
using System.Linq;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    #region Using

    #endregion

    internal class ViewRepository : Repository<View>
    {
        public ViewRepository(TelimenaTelemetryContext dbContext) : base(dbContext)
        {
        }

        private TelimenaTelemetryContext TelimenaPortalContext => this.DbContext as TelimenaTelemetryContext;

        public override void Add(View objectToAdd)
        {
            objectToAdd.FirstReportedDate = DateTime.UtcNow;
            this.TelimenaPortalContext.Views.Add(objectToAdd);
        }

        public void AddUsage(ViewTelemetrySummary objectToAdd)
        {
            this.TelimenaPortalContext.ViewTelemetrySummaries.Add(objectToAdd);
        }

        public View GetView(string viewName, Program program)
        {
            return this.TelimenaPortalContext.Views.FirstOrDefault(x => x.ProgramId == program.Id && x.Name == viewName);
        }

        public ViewTelemetrySummary GetUsage(View view, ClientAppUser clientAppUser)
        {
            return this.TelimenaPortalContext.ViewTelemetrySummaries.FirstOrDefault(x => x.View.Id == view.Id && x.ClientAppUserId == clientAppUser.Id);
        }
    }
}