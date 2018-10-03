namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedIsPublic2 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Updaters", "IsPublic");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Updaters", "IsPublic", c => c.Boolean(nullable: false));
        }
    }
}
