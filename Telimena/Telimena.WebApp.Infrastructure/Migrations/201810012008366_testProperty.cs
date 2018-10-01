namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class testProperty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Programs", "TestProperty", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Programs", "TestProperty");
        }
    }
}
