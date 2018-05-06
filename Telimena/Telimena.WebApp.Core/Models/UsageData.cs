namespace Telimena.WebApp.Core.Models
{
    using System;

    public abstract class UsageData
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public virtual UserInfo UserInfo { get; set; }
        public int? UserInfoId { get; set; }
        public int Count { get; set; } 
    }
}