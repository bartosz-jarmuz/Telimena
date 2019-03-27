namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedDescriptionFieldToUpdater : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Updaters", "Description", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Updaters", "Description");
        }
    }
}
