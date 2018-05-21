namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using Core.Models;
    using Database;

    public class FunctionRepository : Repository<Function>, IFunctionRepository
    {
        private TelimenaContext TelimenaContext => this.DbContext as TelimenaContext;

        public Function GetFunctionOrAddIfNotExists(string functionName, Program program)
        {
            var func = this.TelimenaContext.Functions.FirstOrDefault(x => x.Name == functionName && x.Program.Name == program.Name);
            if (func == null)
            {
                func = new Function()
                {
                    Name = functionName,
                    Program = program,
                    ProgramId = program.Id
                };
                this.Add(func);
            }

            return func;
        }


        public FunctionUsage GetFunctionUsageOrAddIfNotExists(string functionName, Program program, ClientAppUser clientAppUser)
        {
            var function = this.GetFunctionOrAddIfNotExists(functionName, program);
            var usageData = this.TelimenaContext.FunctionUsages.FirstOrDefault(x => x.Function.Name == function.Name && x.ClientAppUser.UserName == clientAppUser.UserName);
            if (usageData == null)
            {
                usageData = new FunctionUsage()
                {
                    ClientAppUser = clientAppUser,
                    Function = function,
                    FunctionId = function.Id
                };
                this.AddFunctionUsage(usageData);
            }

            return usageData;
        }







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