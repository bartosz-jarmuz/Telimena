namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Client;
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

        public ClientAppUser GetUserInfoOrAddIfNotExists(UserInfo userDto)
        {
            var user = this.TelimenaContext.AppUsers.FirstOrDefault(x => x.UserName == userDto.UserName);
            if (user == null)
            {
                user = Mapper.Map<ClientAppUser>(userDto);
                this.Add(user);
            }

            return user;
        }

        public  ClientAppUser GetUserInfoOrAddIfNotExists(string userName)
        {
            var user = this.TelimenaContext.AppUsers.FirstOrDefault(x => x.UserName == userName);
            if (user == null)
            {
                user = new ClientAppUser()
                {
                    UserName = userName
                };
                this.Add(user);
            }

            return user;
        }
    }
}