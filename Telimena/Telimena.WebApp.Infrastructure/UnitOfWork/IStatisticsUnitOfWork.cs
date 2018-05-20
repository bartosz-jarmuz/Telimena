using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.UnitOfWork
{
    using Repository;

    public interface IStatisticsUnitOfWork
    {
        IClientAppUserRepository ClientAppUserRepository { get; }

        IProgramRepository ProgramRepository { get;  }

        IFunctionRepository FunctionRepository { get; }

        Task CompleteAsync();
    }
}
