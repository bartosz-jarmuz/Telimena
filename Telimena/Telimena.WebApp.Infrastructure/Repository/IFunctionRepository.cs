namespace Telimena.WebApp.Infrastructure.Repository
{
    using Core.Models;

    public interface IFunctionRepository : IRepository<Function>
    {
        void AddUsage(FunctionUsage objectToAdd);
        Function GetFunction(string functionName, Program program);
        FunctionUsage GetUsage(Function function, ClientAppUser clientAppUser);
    }
}