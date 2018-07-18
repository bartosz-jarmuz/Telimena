// -----------------------------------------------------------------------
//  <copyright file="FunctionRepository.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    #region Using
    using System;
    using System.Data.Entity;
    using System.Linq;
    using Core.Models;
    using Database;
    #endregion

    internal class FunctionRepository : Repository<Function>, IFunctionRepository
    {
        public FunctionRepository(DbContext dbContext) : base(dbContext)
        {
        }

        private TelimenaContext TelimenaContext => this.DbContext as TelimenaContext;

        public override void Add(Function objectToAdd)
        {
            objectToAdd.RegisteredDate = DateTime.UtcNow;
            this.TelimenaContext.Functions.Add(objectToAdd);
        }

        public void AddUsage(FunctionUsageSummary objectToAdd)
        {
            this.TelimenaContext.FunctionUsages.Add(objectToAdd);
        }

        public Function GetFunction(string functionName, Program program)
        {
            return this.TelimenaContext.Functions.FirstOrDefault(x => x.ProgramId == program.Id && x.Name == functionName);
        }

        public FunctionUsageSummary GetUsage(Function function, ClientAppUser clientAppUser)
        {
            return this.TelimenaContext.FunctionUsages.FirstOrDefault(x => x.Function.Id == function.Id && x.ClientAppUserId == clientAppUser.Id);
        }
    }
}