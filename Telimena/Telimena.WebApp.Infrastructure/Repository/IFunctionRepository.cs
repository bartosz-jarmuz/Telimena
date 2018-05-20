namespace Telimena.WebApp.Infrastructure.Repository
{
    using Core.Models;

    public interface IFunctionRepository : IRepository<Function>
    {
        void AddFunctionUsage(FunctionUsage objectToAdd);
    }
}