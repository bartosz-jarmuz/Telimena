namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    using System;
    using Core.Models;
    using Database;

    public class ClientAppUserRepository :Repository<ClientAppUser>, IClientAppUserRepository
    {
        private TelimenaContext TelimenaContext => this.DbContext as TelimenaContext;
        public ClientAppUserRepository(TelimenaContext dbContext) : base(dbContext)
        {
        }
        public override void Add(ClientAppUser objectToAdd)
        {
            objectToAdd.RegisteredDate = DateTime.UtcNow;
            this.TelimenaContext.AppUsers.Add(objectToAdd);
        }
    }
}