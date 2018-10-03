namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedIsPublic3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Updaters", "IsPublic", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Updaters", "IsPublic");
        }
    }
}
