
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
        public ViewRepository(DbContext dbContext) : base(dbContext)
        {
        }

        private TelimenaContext TelimenaContext => this.DbContext as TelimenaContext;

        public override void Add(View objectToAdd)
        {
            objectToAdd.FirstReportedDate = DateTime.UtcNow;
            this.TelimenaContext.Views.Add(objectToAdd);
        }

        public void AddUsage(ViewTelemetrySummary objectToAdd)
        {
            this.TelimenaContext.ViewUsages.Add(objectToAdd);
        }

        public View GetView(string viewName, Program program)
        {
            return this.TelimenaContext.Views.FirstOrDefault(x => x.ProgramId == program.Id && x.Name == viewName);
        }

        public ViewTelemetrySummary GetUsage(View view, ClientAppUser clientAppUser)
        {
            return this.TelimenaContext.ViewUsages.FirstOrDefault(x => x.View.Id == view.Id && x.ClientAppUserId == clientAppUser.Id);
        }
    }
}