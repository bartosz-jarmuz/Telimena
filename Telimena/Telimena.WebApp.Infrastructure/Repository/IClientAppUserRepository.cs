namespace Telimena.WebApp.Infrastructure.Repository
{
    using Client;
    using Core.Models;

    public interface IClientAppUserRepository : IRepository<ClientAppUser>
    {
        ClientAppUser GetUserInfoOrAddIfNotExists(UserInfo userDto);
        ClientAppUser GetUserInfoOrAddIfNotExists(string userName);
    }
}