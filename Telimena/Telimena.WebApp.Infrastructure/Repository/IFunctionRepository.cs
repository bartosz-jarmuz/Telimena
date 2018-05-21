namespace Telimena.WebApp.Infrastructure.Repository
{
    using Core.Models;

    public interface IFunctionRepository : IRepository<Function>
    {
        void AddFunctionUsage(FunctionUsage objectToAdd);
        Function GetFunctionOrAddIfNotExists(string functionName, Program program);
        FunctionUsage GetFunctionUsageOrAddIfNotExists(string functionName, Program program, ClientAppUser clientAppUser);
    }
}