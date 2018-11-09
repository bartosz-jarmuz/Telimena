namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedmoreversionproperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AssemblyVersions", "ProductVersion", c => c.String());
            AddColumn("dbo.AssemblyVersions", "AssemblyFileVersion", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AssemblyVersions", "AssemblyFileVersion");
            DropColumn("dbo.AssemblyVersions", "ProductVersion");
        }
    }
}
