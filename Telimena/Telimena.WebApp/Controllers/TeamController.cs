using System.Linq;
using MvcAuditLogger;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
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
        public async Task<ActionResult> Index(int id)
        {
            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);

            var getTeamResult = await this.GetTeam(id, telimenaUser.Id);
            if (getTeamResult.ErrorResult!= null)
            {
                return getTeamResult.ErrorResult;
            }

            var model = new TeamViewModel() {Name = getTeamResult.Team.Name, TeamId = getTeamResult.Team.Id};

            foreach (TelimenaUser user in getTeamResult.Team.AssociatedUsers)
            {
                model.TeamMembers.Add(new SelectListItem(){Value = user.Email, Text = user.DisplayName});
            }

            return this.View("Index", model);
        }

        private class GetTeamResult
        {
            public DeveloperTeam Team { get; set; }

            public ActionResult ErrorResult { get; set; }
        }

        private async Task<GetTeamResult> GetTeam(int teamId, string userId)
        {
            DeveloperTeam team = await this.unitOfWork.Developers.FirstOrDefaultAsync(x => x.Id == teamId).ConfigureAwait(false);
            var result = new GetTeamResult();
            if (team == null)
            {
                result.ErrorResult = this.HttpNotFound();
            }

            if (team.AssociatedUsers.All(x => x.Id != userId))
            {
                result.ErrorResult = this.RedirectToAction("AccessDenied", "Error", new { roles = "Member of the team"});
            }

            result.Team = team;
            return result;
        }

        private async Task<GetTeamResult> GetTeamManagedByUser(int teamId, string userId)
        {
            DeveloperTeam team = await this.unitOfWork.Developers.FirstOrDefaultAsync(x => x.Id == teamId).ConfigureAwait(false);
            var result = new GetTeamResult();
            if (team == null)
            {
                result.ErrorResult = this.HttpNotFound();
            }

            if (team.MainUserId  != userId)
            {
                result.ErrorResult = this.RedirectToAction("AccessDenied", "Error");
            }

            result.Team = team;
            return result;
        }

        /// <summary>
        /// Adds the member.
        /// </summary>
        /// <param name="teamId">The teamId.</param>
        /// <param name="newName"></param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [Audit(AuditingLevel.BasicData)]
        [HttpPost]
        public async Task<ActionResult> RenameTeam(int teamId, string newName)
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
            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);

            var getTeamResult = await this.GetTeam(teamId, telimenaUser.Id);
            if (getTeamResult.ErrorResult != null)
            {
                return getTeamResult.ErrorResult;
            }

            var existingTeam = await this.unitOfWork.Developers.FirstOrDefaultAsync(x => x.Name == newName).ConfigureAwait(false);
            if (existingTeam != null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Conflict, $"A [{existingTeam.Name}] team already exists");
            }

            getTeamResult.Team.Name = newName;
            await this.unitOfWork.CompleteAsync().ConfigureAwait(false);
            return new HttpStatusCodeResult(HttpStatusCode.OK, $"Renamed team to [{getTeamResult.Team.Name}]");
        }

        /// <summary>
        /// Adds the member.
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="email">The email.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [Audit(AuditingLevel.BasicData)]
        [HttpPost]
        public async Task<ActionResult> AddMember(int teamId, string email)
        {
            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);

            var getTeamResult = await this.GetTeamManagedByUser(teamId, telimenaUser.Id);
            if (getTeamResult.ErrorResult != null)
            {
                return getTeamResult.ErrorResult;
            }

            TelimenaUser newUser = this.unitOfWork.Users.FirstOrDefault(x => x.Email == email);

            if (newUser == null)
            {
                return this.HttpNotFound($"User with email [{email}] does not exist");
            }



            getTeamResult.Team.AssociateUser(newUser);
            await this.unitOfWork.CompleteAsync().ConfigureAwait(false);
            return new HttpStatusCodeResult(HttpStatusCode.OK, $"Added user [{newUser.Email}] to [{getTeamResult.Team.Name}]");
        }

        /// <summary>
        /// Adds the member.
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="email">The email.</param>
        /// <returns>Task&lt;ActionResult&gt;.</returns>
        [Audit]
        [HttpDelete]
        public async Task<ActionResult> RemoveMember(int teamId, string email)
        {
            TelimenaUser telimenaUser = this.unitOfWork.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);

            var getTeamResult = await this.GetTeamManagedByUser(teamId, telimenaUser.Id);
            if (getTeamResult.ErrorResult != null)
            {
                return getTeamResult.ErrorResult;
            }

            TelimenaUser userToBeRemoved = this.unitOfWork.Users.FirstOrDefault(x => x.Email == email);

            if (userToBeRemoved == null)
            {
                return this.HttpNotFound($"User with email [{email}] does not exist");
            }

            getTeamResult.Team.RemoveAssociatedUser(userToBeRemoved);
            await this.unitOfWork.CompleteAsync().ConfigureAwait(false);

            return new HttpStatusCodeResult(HttpStatusCode.OK, $"Removed user [{userToBeRemoved.Email}] from [{getTeamResult.Team.Name}]");
        }
    }
}