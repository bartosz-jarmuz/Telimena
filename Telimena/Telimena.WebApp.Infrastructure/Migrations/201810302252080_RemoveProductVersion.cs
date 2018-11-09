namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveProductVersion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AssemblyVersions", "FileVersion", c => c.String());
            DropColumn("dbo.AssemblyVersions", "ProductVersion");
            DropColumn("dbo.AssemblyVersions", "AssemblyFileVersion");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AssemblyVersions", "AssemblyFileVersion", c => c.String());
            AddColumn("dbo.AssemblyVersions", "ProductVersion", c => c.String());
            DropColumn("dbo.AssemblyVersions", "FileVersion");
        }
    }
}
