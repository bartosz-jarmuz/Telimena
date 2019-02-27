using System;

namespace Telimena.Portal.Api.Models.DTO
{
    public class UpdaterDto
    {
        public Guid Id { get; set; }
        public string InternalName {get; set; }
        public string FileName { get; set; }
        public bool IsPublic { get; set; }
        public string DeveloperTeamName { get; set; }
    }
}