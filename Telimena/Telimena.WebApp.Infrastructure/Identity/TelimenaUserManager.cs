using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.WebApp.Infrastructure.Identity
{
    public interface ITelimenaUserManager
    {
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
        Task<IdentityResult> AccessFailedAsync(string userId);
        Task<IdentityResult> AddClaimAsync(string userId, Claim claim);
        Task<IdentityResult> AddLoginAsync(string userId, UserLoginInfo login);
        Task<IdentityResult> AddPasswordAsync(string userId, string password);
        Task<IdentityResult> AddToRoleAsync(string userId, string role);
        Task<IdentityResult> AddToRolesAsync(string userId, params string[] roles);
        Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<IdentityResult> ChangePhoneNumberAsync(string userId, string phoneNumber, string token);
        Task<bool> CheckPasswordAsync(TelimenaUser user, string password);
        Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
        Task<IdentityResult> CreateAsync(TelimenaUser user);
        Task<IdentityResult> CreateAsync(TelimenaUser user, string password);
        Task<ClaimsIdentity> CreateIdentityAsync(TelimenaUser user, string authenticationType);
        Task<IdentityResult> DeleteAsync(TelimenaUser user);
        void Dispose();
        Task<TelimenaUser> FindAsync(string userName, string password);
        Task<TelimenaUser> FindAsync(UserLoginInfo login);
        Task<TelimenaUser> FindByEmailAsync(string email);
        Task<TelimenaUser> FindByIdAsync(string userId);
        Task<TelimenaUser> FindByNameAsync(string userName);
        Task<string> GenerateChangePhoneNumberTokenAsync(string userId, string phoneNumber);
        Task<string> GenerateEmailConfirmationTokenAsync(string userId);
        Task<string> GeneratePasswordResetTokenAsync(string userId);
        Task<string> GenerateTwoFactorTokenAsync(string userId, string twoFactorProvider);
        Task<string> GenerateUserTokenAsync(string purpose, string userId);
        Task<int> GetAccessFailedCountAsync(string userId);
        Task<IList<Claim>> GetClaimsAsync(string userId);
        Task<string> GetEmailAsync(string userId);
        Task<bool> GetLockoutEnabledAsync(string userId);
        Task<DateTimeOffset> GetLockoutEndDateAsync(string userId);
        Task<IList<UserLoginInfo>> GetLoginsAsync(string userId);
        Task<string> GetPhoneNumberAsync(string userId);
        Task<IList<string>> GetRolesAsync(string userId);
        Task<string> GetSecurityStampAsync(string userId);
        Task<bool> GetTwoFactorEnabledAsync(string userId);
        Task<IList<string>> GetValidTwoFactorProvidersAsync(string userId);
        Task<bool> HasPasswordAsync(string userId);
        Task<bool> IsEmailConfirmedAsync(string userId);
        Task<bool> IsInRoleAsync(string userId, string role);
        Task<bool> IsLockedOutAsync(string userId);
        Task<bool> IsPhoneNumberConfirmedAsync(string userId);
        Task<IdentityResult> NotifyTwoFactorTokenAsync(string userId, string twoFactorProvider, string token);
        void RegisterTwoFactorProvider(string twoFactorProvider, IUserTokenProvider<TelimenaUser, string> provider);
        Task<IdentityResult> RemoveClaimAsync(string userId, Claim claim);
        Task<IdentityResult> RemoveFromRoleAsync(string userId, string role);
        Task<IdentityResult> RemoveFromRolesAsync(string userId, params string[] roles);
        Task<IdentityResult> RemoveLoginAsync(string userId, UserLoginInfo login);
        Task<IdentityResult> RemovePasswordAsync(string userId);
        Task<IdentityResult> ResetAccessFailedCountAsync(string userId);
        Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword);
        Task SendEmailAsync(string userId, string subject, string body);
        Task SendSmsAsync(string userId, string message);
        Task<IdentityResult> SetEmailAsync(string userId, string email);
        Task<IdentityResult> SetLockoutEnabledAsync(string userId, bool enabled);
        Task<IdentityResult> SetLockoutEndDateAsync(string userId, DateTimeOffset lockoutEnd);
        Task<IdentityResult> SetPhoneNumberAsync(string userId, string phoneNumber);
        Task<IdentityResult> SetTwoFactorEnabledAsync(string userId, bool enabled);
        Task<IdentityResult> UpdateAsync(TelimenaUser user);
        Task<IdentityResult> UpdateSecurityStampAsync(string userId);
        Task<bool> VerifyChangePhoneNumberTokenAsync(string userId, string token, string phoneNumber);
        Task<bool> VerifyTwoFactorTokenAsync(string userId, string twoFactorProvider, string token);
        Task<bool> VerifyUserTokenAsync(string userId, string purpose, string token);
    }

    public class TelimenaUserManager : UserManager<TelimenaUser>, ITelimenaUserManager
    {
        public TelimenaUserManager(IUserStore<TelimenaUser> store) : base(store)
        {
        }

        public static TelimenaUserManager Create(IdentityFactoryOptions<TelimenaUserManager> options, IOwinContext context)
        {
            TelimenaUserManager manager = new TelimenaUserManager(new UserStore<TelimenaUser>(context.Get<TelimenaPortalContext>()));

            return manager;
        }
    }
}