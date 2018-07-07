namespace Telimena.WebApp.Infrastructure.Identity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Core.Models;
    using Database;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin;

    public interface ITelimenaUserManager 
    {
        void Dispose();
        Task<ClaimsIdentity> CreateIdentityAsync(TelimenaUser user, string authenticationType);
        Task<IdentityResult> CreateAsync(TelimenaUser user);
        Task<IdentityResult> UpdateAsync(TelimenaUser user);
        Task<IdentityResult> DeleteAsync(TelimenaUser user);
        Task<TelimenaUser> FindByIdAsync(string userId);
        Task<TelimenaUser> FindByNameAsync(string userName);
        Task<IdentityResult> CreateAsync(TelimenaUser user, string password);
        Task<TelimenaUser> FindAsync(string userName, string password);
        Task<bool> CheckPasswordAsync(TelimenaUser user, string password);
        Task<bool> HasPasswordAsync(string userId);
        Task<IdentityResult> AddPasswordAsync(string userId, string password);
        Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<IdentityResult> RemovePasswordAsync(string userId);
        Task<string> GetSecurityStampAsync(string userId);
        Task<IdentityResult> UpdateSecurityStampAsync(string userId);
        Task<string> GeneratePasswordResetTokenAsync(string userId);
        Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword);
        Task<TelimenaUser> FindAsync(UserLoginInfo login);
        Task<IdentityResult> RemoveLoginAsync(string userId, UserLoginInfo login);
        Task<IdentityResult> AddLoginAsync(string userId, UserLoginInfo login);
        Task<IList<UserLoginInfo>> GetLoginsAsync(string userId);
        Task<IdentityResult> AddClaimAsync(string userId, Claim claim);
        Task<IdentityResult> RemoveClaimAsync(string userId, Claim claim);
        Task<IList<Claim>> GetClaimsAsync(string userId);
        Task<IdentityResult> AddToRoleAsync(string userId, string role);
        Task<IdentityResult> AddToRolesAsync(string userId, params string[] roles);
        Task<IdentityResult> RemoveFromRolesAsync(string userId, params string[] roles);
        Task<IdentityResult> RemoveFromRoleAsync(string userId, string role);
        Task<IList<string>> GetRolesAsync(string userId);
        Task<bool> IsInRoleAsync(string userId, string role);
        Task<string> GetEmailAsync(string userId);
        Task<IdentityResult> SetEmailAsync(string userId, string email);
        Task<TelimenaUser> FindByEmailAsync(string email);
        Task<string> GenerateEmailConfirmationTokenAsync(string userId);
        Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
        Task<bool> IsEmailConfirmedAsync(string userId);
        Task<string> GetPhoneNumberAsync(string userId);
        Task<IdentityResult> SetPhoneNumberAsync(string userId, string phoneNumber);
        Task<IdentityResult> ChangePhoneNumberAsync(string userId, string phoneNumber, string token);
        Task<bool> IsPhoneNumberConfirmedAsync(string userId);
        Task<string> GenerateChangePhoneNumberTokenAsync(string userId, string phoneNumber);
        Task<bool> VerifyChangePhoneNumberTokenAsync(string userId, string token, string phoneNumber);
        Task<bool> VerifyUserTokenAsync(string userId, string purpose, string token);
        Task<string> GenerateUserTokenAsync(string purpose, string userId);
        void RegisterTwoFactorProvider(string twoFactorProvider, IUserTokenProvider<TelimenaUser, string> provider);
        Task<IList<string>> GetValidTwoFactorProvidersAsync(string userId);
        Task<bool> VerifyTwoFactorTokenAsync(string userId, string twoFactorProvider, string token);
        Task<string> GenerateTwoFactorTokenAsync(string userId, string twoFactorProvider);
        Task<IdentityResult> NotifyTwoFactorTokenAsync(string userId, string twoFactorProvider, string token);
        Task<bool> GetTwoFactorEnabledAsync(string userId);
        Task<IdentityResult> SetTwoFactorEnabledAsync(string userId, bool enabled);
        Task SendEmailAsync(string userId, string subject, string body);
        Task SendSmsAsync(string userId, string message);
        Task<bool> IsLockedOutAsync(string userId);
        Task<IdentityResult> SetLockoutEnabledAsync(string userId, bool enabled);
        Task<bool> GetLockoutEnabledAsync(string userId);
        Task<DateTimeOffset> GetLockoutEndDateAsync(string userId);
        Task<IdentityResult> SetLockoutEndDateAsync(string userId, DateTimeOffset lockoutEnd);
        Task<IdentityResult> AccessFailedAsync(string userId);
        Task<IdentityResult> ResetAccessFailedCountAsync(string userId);
        Task<int> GetAccessFailedCountAsync(string userId);
        IPasswordHasher PasswordHasher { get; set; }
        IIdentityValidator<TelimenaUser> UserValidator { get; set; }
        IIdentityValidator<string> PasswordValidator { get; set; }
        IClaimsIdentityFactory<TelimenaUser, string> ClaimsIdentityFactory { get; set; }
        IIdentityMessageService EmailService { get; set; }
        IIdentityMessageService SmsService { get; set; }
        IUserTokenProvider<TelimenaUser, string> UserTokenProvider { get; set; }
        bool UserLockoutEnabledByDefault { get; set; }
        int MaxFailedAccessAttemptsBeforeLockout { get; set; }
        TimeSpan DefaultAccountLockoutTimeSpan { get; set; }
        bool SupportsUserTwoFactor { get; }
        bool SupportsUserPassword { get; }
        bool SupportsUserSecurityStamp { get; }
        bool SupportsUserRole { get; }
        bool SupportsUserLogin { get; }
        bool SupportsUserEmail { get; }
        bool SupportsUserPhoneNumber { get; }
        bool SupportsUserClaim { get; }
        bool SupportsUserLockout { get; }
        bool SupportsQueryableUsers { get; }
        IQueryable<TelimenaUser> Users { get; }
        IDictionary<string, IUserTokenProvider<TelimenaUser, string>> TwoFactorProviders { get; }
    }

    public class TelimenaUserManager : UserManager<TelimenaUser>, ITelimenaUserManager
    {
        public TelimenaUserManager(IUserStore<TelimenaUser> store)
            : base(store)
        {
        }

        //#region Overrides of UserManager<TelimenaUser,string>
        //public override async Task<TelimenaUser> FindByIdAsync(string userId)
        //{
        //    var user = await base.FindByIdAsync(userId);
        //    await this.LoadRoles(user);
        //    return user;
        //}

        //private async Task LoadRoles(TelimenaUser user)
        //{
        //    if (user != null)
        //    {
        //        var roleStore = this.Store as IUserRoleStore<TelimenaUser, string>;
        //        user.RoleNames = await roleStore.GetRolesAsync(user);
        //    }
        //}

        //public override async Task<TelimenaUser> FindByNameAsync(string userName)
        //{
        //    var user = await base.FindByNameAsync(userName);
        //    await this.LoadRoles(user);
        //    return user;
        //}

        //public override async Task<TelimenaUser> FindByEmailAsync(string email)
        //{
        //    var user = await base.FindByEmailAsync(email);
        //    await this.LoadRoles(user);
        //    return user;
        //}
        // #endregion

        public static TelimenaUserManager Create(
            IdentityFactoryOptions<TelimenaUserManager> options, IOwinContext context)
        {
            var manager = new TelimenaUserManager(
                new UserStore<TelimenaUser>(context.Get<TelimenaContext>()));

            return manager;
        }
    }
}