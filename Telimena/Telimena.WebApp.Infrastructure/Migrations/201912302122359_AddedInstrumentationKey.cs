namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedInstrumentationKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Programs", "InstrumentationKey", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Programs", "InstrumentationKey");
        }
    }
}
