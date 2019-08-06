using MvcAuditLogger;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Telimena.Portal.Utils;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using Telimena.WebApp.Models.Team;

namespace Telimena.WebApp.Controllers
{

    /// <summary>
    /// Class TeamController.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    [TelimenaAuthorize(Roles = TelimenaRoles.Developer)]
        public class TeamController : Controller
        {
            private readonly ITeamUnitOfWork unitOfWork;

            /// <summary>
        /// Initializes a new instance of the <see cref="TeamController"/> class.
        /// </summary>
        public TeamController( ITeamUnitOfWork unitOfWork)
            {
                this.unitOfWork = unitOfWork;
            }


        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>ActionResult.</returns>
        [Audit]
        public async Task<ActionResult> Index()
        {
            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);

            DeveloperTeam team = await this.unitOfWork.Developers.FirstOrDefaultAsync(x => x.MainUser.Id == telimenaUser.Id).ConfigureAwait(false);

            var model = new TeamViewModel() {Name = team.Name};

            foreach (TelimenaUser user in team.AssociatedUsers)
            {
                model.TeamMembers.Add(new SelectListItem(){Value = user.Email, Text = user.DisplayName});
            }

            return this.View("Index", model);
        }

        /// <summary>
        /// Adds the member.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [Audit(AuditingLevel.BasicData)]
        [HttpPost]
        public async Task<ActionResult> RenameTeam(string newName)
        {
            if (!newName.IsUrlFriendly())
            {
                var properName = newName.MakeUrlFriendly();
                var properNameHint = "";
                if (properName != null)
                {
                    properNameHint = $" Proposed name: {properName}";
                }

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, $"Team name should only contain letters and numbers or hyphens. " +
                                                                         $"Also it needs to begin and end with a letter or digit.{properNameHint}");
            }

            var existingTeam = await this.unitOfWork.Developers.FirstOrDefaultAsync(x => x.Name == newName).ConfigureAwait(false);
            if (existingTeam != null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Conflict, $"A [{existingTeam.Name}] team already exists");
            }
            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);

            DeveloperTeam team = await this.unitOfWork.Developers.FirstOrDefaultAsync(x => x.MainUser.Id == telimenaUser.Id).ConfigureAwait(false);
            team.Name = newName;
            await this.unitOfWork.CompleteAsync().ConfigureAwait(false);
            return new HttpStatusCodeResult(HttpStatusCode.OK, $"Renamed team to [{team.Name}]");
        }

        /// <summary>
        /// Adds the member.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [Audit(AuditingLevel.BasicData)]
        [HttpPost]
        public async Task<ActionResult> AddMember(string email)
        {
            TelimenaUser newUser = this.unitOfWork.Users.FirstOrDefault(x => x.Email == email);

            if (newUser == null)
            {
                return this.HttpNotFound($"User with email [{email}] does not exist");
            }

            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);

            DeveloperTeam team = await this.unitOfWork.Developers.FirstOrDefaultAsync(x => x.MainUser.Id == telimenaUser.Id).ConfigureAwait(false);

            team.AssociateUser(newUser);
            await this.unitOfWork.CompleteAsync().ConfigureAwait(false);
            return new HttpStatusCodeResult(HttpStatusCode.OK, $"Added user [{newUser.Email}] to [{team.Name}]");
        }

        /// <summary>
        /// Adds the member.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [Audit]
        [HttpDelete]
        public async Task<ActionResult> RemoveMember(string email)
        {
            TelimenaUser userToBeRemoved = this.unitOfWork.Users.FirstOrDefault(x => x.Email == email);

            if (userToBeRemoved == null)
            {
                return this.HttpNotFound($"User with email [{email}] does not exist");
            }
            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);

            DeveloperTeam team = await this.unitOfWork.Developers.FirstOrDefaultAsync(x => x.MainUser.Id == telimenaUser.Id).ConfigureAwait(false);

            team.RemoveAssociatedUser(userToBeRemoved);
            await this.unitOfWork.CompleteAsync().ConfigureAwait(false);

            return new HttpStatusCodeResult(HttpStatusCode.OK, $"Removed user [{userToBeRemoved.Email}] from [{team.Name}]");
        }
    }
}