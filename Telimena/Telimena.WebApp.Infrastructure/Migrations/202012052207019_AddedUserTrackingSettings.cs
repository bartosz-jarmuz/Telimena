namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUserTrackingSettings : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Programs", "UserTrackingSettings", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Programs", "UserTrackingSettings");
        }
    }
}
