namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    using System;
    using System.Data.Entity;
    using Core.Models;
    using Database;

    public class FunctionRepository : Repository<Function>, IFunctionRepository
    {
        private TelimenaContext TelimenaContext => this.DbContext as TelimenaContext;

        public override void Add(Function objectToAdd)
        {
            objectToAdd.RegisteredDate = DateTime.UtcNow;
            this.TelimenaContext.Functions.Add(objectToAdd);
        }

        public void AddFunctionUsage(FunctionUsage objectToAdd)
        {
            this.TelimenaContext.FunctionUsages.Add(objectToAdd);
        }

        public FunctionRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}