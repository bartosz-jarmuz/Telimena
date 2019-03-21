namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedVersionToProgramPackageInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProgramPackageInfoes", "Version", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProgramPackageInfoes", "Version");
        }
    }
}
