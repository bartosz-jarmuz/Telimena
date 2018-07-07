using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.WebApp.Infrastructure.UnitOfWork
{
    using Core.Models;
    using Repository;

    public interface IStatisticsUnitOfWork
    {
        IRepository<ClientAppUser> ClientAppUsers { get; }
        IRepository<AssemblyVersion> Versions { get; }

        IProgramRepository Programs { get;  }

        IFunctionRepository Functions { get; }
        IRepository<DeveloperAccount> Developers { get; set; }
        ITelimenaToolkitRepository TelimenaToolkitDataRepository { get; set; }
        Task CompleteAsync();
    }
}
