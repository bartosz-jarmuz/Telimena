namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReaddedInstrumentationKey : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Programs", "InstrumentationKey", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Programs", "InstrumentationKey", c => c.Guid(nullable: false));
        }
    }
}
