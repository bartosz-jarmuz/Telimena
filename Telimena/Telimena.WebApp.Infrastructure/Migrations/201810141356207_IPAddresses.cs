namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IPAddresses : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ClientAppUsers", "IpAddressesString", c => c.String());
            AddColumn("dbo.FunctionUsageDetails", "IpAddress", c => c.String());
            AddColumn("dbo.ProgramUsageDetails", "IpAddress", c => c.String());
            DropColumn("dbo.ClientAppUsers", "IpAddress");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ClientAppUsers", "IpAddress", c => c.String());
            DropColumn("dbo.ProgramUsageDetails", "IpAddress");
            DropColumn("dbo.FunctionUsageDetails", "IpAddress");
            DropColumn("dbo.ClientAppUsers", "IpAddressesString");
        }
    }
}
