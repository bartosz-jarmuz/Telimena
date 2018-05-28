namespace Telimena.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Client;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Repository;

    public class FakeFunctionsRepo : FakeRepo<Function>, IFunctionRepository
    {
        public List<FunctionUsage> Usages { get; set; } = new List<FunctionUsage>();

        #region Implementation of IFunctionRepository
        public void AddUsage(FunctionUsage objectToAdd)
        {
            this.Usages.Add(objectToAdd);
        }

        public Function GetFunction(string functionName, Program program)
        {
            return this.Data.FirstOrDefault(x => x.Name == functionName && x.Program == program);
        }

        public FunctionUsage GetUsage(Function function, ClientAppUser clientAppUser)
        {
            return this.Usages.FirstOrDefault(x => x.FunctionId == function.Id && x.ClientAppUserId == clientAppUser.Id);

        }
        #endregion
    }

}