namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Programs", "TestProperty");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Programs", "TestProperty", c => c.DateTime(nullable: false));
        }
    }
}
