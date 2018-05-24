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
        IClientAppUserRepository ClientAppUsers { get; }

        IProgramRepository Programs { get;  }

        IFunctionRepository Functions { get; }

        Task CompleteAsync();
    }
}
