namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVersionToProgramPackageInfo : DbMigration
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
