namespace Telimena.WebApp.Infrastructure.Migrations.Telemetry
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClientAppUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PublicId = c.Guid(nullable: false),
                        FirstSeenDate = c.DateTimeOffset(nullable: false, precision: 7),
                        UserIdentifier = c.String(),
                        AuthenticatedUserIdentifier = c.String(),
                        Email = c.String(),
                        MachineName = c.String(),
                        IpAddressesString = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ClusterId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ProgramId = c.Int(nullable: false),
                        FirstReportedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        Program_RootObjectId = c.Int(),
                    })
                .PrimaryKey(t => t.Id, clustered: false)
                .ForeignKey("dbo.TelemetryRootObjects", t => t.Program_RootObjectId)
                .Index(t => t.Id, unique: true)
                .Index(t => t.ClusterId, clustered: true)
                .Index(t => t.Program_RootObjectId);
            
            CreateTable(
                "dbo.TelemetryRootObjects",
                c => new
                    {
                        RootObjectId = c.Int(nullable: false, identity: true),
                        ProgramId = c.Int(nullable: false),
                        TelemetryKey = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.RootObjectId)
                .Index(t => t.ProgramId, unique: true)
                .Index(t => t.TelemetryKey);
            
            CreateTable(
                "dbo.Views",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ClusterId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ProgramId = c.Int(nullable: false),
                        FirstReportedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        Program_RootObjectId = c.Int(),
                    })
                .PrimaryKey(t => t.Id, clustered: false)
                .ForeignKey("dbo.TelemetryRootObjects", t => t.Program_RootObjectId)
                .Index(t => t.Id, unique: true)
                .Index(t => t.ClusterId, clustered: true)
                .Index(t => t.Program_RootObjectId);
            
            CreateTable(
                "dbo.ViewTelemetrySummaries",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ViewId = c.Guid(nullable: false),
                        ClusterId = c.Int(nullable: false, identity: true),
                        LastTelemetryUpdateTimestamp = c.DateTimeOffset(nullable: false, precision: 7),
                        ClientAppUserId = c.Int(),
                        SummaryCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id, clustered: false)
                .ForeignKey("dbo.ClientAppUsers", t => t.ClientAppUserId)
                .ForeignKey("dbo.Views", t => t.ViewId, cascadeDelete: true)
                .Index(t => t.Id, unique: true)
                .Index(t => t.ViewId)
                .Index(t => t.ClusterId, clustered: true)
                .Index(t => t.ClientAppUserId);
            
            CreateTable(
                "dbo.ViewTelemetryDetails",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ClusterId = c.Int(nullable: false, identity: true),
                        Sequence = c.String(),
                        IpAddress = c.String(),
                        AssemblyVersion = c.String(),
                        FileVersion = c.String(),
                        Timestamp = c.DateTimeOffset(nullable: false, precision: 7),
                        TelemetrySummaryId = c.Guid(nullable: false),
                        EntryKey = c.String(),
                        UserIdentifier = c.String(),
                    })
                .PrimaryKey(t => t.Id, clustered: false)
                .ForeignKey("dbo.ViewTelemetrySummaries", t => t.TelemetrySummaryId, cascadeDelete: true)
                .Index(t => t.Id, unique: true)
                .Index(t => t.ClusterId, clustered: true)
                .Index(t => t.TelemetrySummaryId);
            
            CreateTable(
                "dbo.ViewTelemetryUnits",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ClusterId = c.Int(nullable: false, identity: true),
                        Key = c.String(maxLength: 50),
                        ValueString = c.String(),
                        ValueDouble = c.Double(nullable: false),
                        UnitType = c.Int(nullable: false),
                        TelemetryDetail_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id, clustered: false)
                .ForeignKey("dbo.ViewTelemetryDetails", t => t.TelemetryDetail_Id)
                .Index(t => t.Id, unique: true)
                .Index(t => t.ClusterId, clustered: true)
                .Index(t => t.TelemetryDetail_Id);
            
            CreateTable(
                "dbo.EventTelemetrySummaries",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        EventId = c.Guid(nullable: false),
                        ClusterId = c.Int(nullable: false, identity: true),
                        LastTelemetryUpdateTimestamp = c.DateTimeOffset(nullable: false, precision: 7),
                        ClientAppUserId = c.Int(),
                        SummaryCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id, clustered: false)
                .ForeignKey("dbo.ClientAppUsers", t => t.ClientAppUserId)
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .Index(t => t.Id, unique: true)
                .Index(t => t.EventId)
                .Index(t => t.ClusterId, clustered: true)
                .Index(t => t.ClientAppUserId);
            
            CreateTable(
                "dbo.EventTelemetryDetails",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ClusterId = c.Int(nullable: false, identity: true),
                        Sequence = c.String(),
                        IpAddress = c.String(),
                        AssemblyVersion = c.String(),
                        FileVersion = c.String(),
                        Timestamp = c.DateTimeOffset(nullable: false, precision: 7),
                        TelemetrySummaryId = c.Guid(nullable: false),
                        EntryKey = c.String(),
                        UserIdentifier = c.String(),
                    })
                .PrimaryKey(t => t.Id, clustered: false)
                .ForeignKey("dbo.EventTelemetrySummaries", t => t.TelemetrySummaryId, cascadeDelete: true)
                .Index(t => t.Id, unique: true)
                .Index(t => t.ClusterId, clustered: true)
                .Index(t => t.TelemetrySummaryId);
            
            CreateTable(
                "dbo.EventTelemetryUnits",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ClusterId = c.Int(nullable: false, identity: true),
                        Key = c.String(maxLength: 50),
                        ValueString = c.String(),
                        ValueDouble = c.Double(nullable: false),
                        UnitType = c.Int(nullable: false),
                        TelemetryDetail_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id, clustered: false)
                .ForeignKey("dbo.EventTelemetryDetails", t => t.TelemetryDetail_Id)
                .Index(t => t.Id, unique: true)
                .Index(t => t.ClusterId, clustered: true)
                .Index(t => t.TelemetryDetail_Id);
            
            CreateTable(
                "dbo.ExceptionInfoes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ExceptionId = c.Long(nullable: false),
                        ExceptionOuterId = c.Long(nullable: false),
                        TypeName = c.String(),
                        Message = c.String(),
                        HasFullStack = c.Boolean(nullable: false),
                        ParsedStack = c.String(),
                        Sequence = c.String(),
                        ClusterId = c.Int(nullable: false, identity: true),
                        Note = c.String(),
                        ProgramId = c.Int(nullable: false),
                        UserName = c.String(),
                        Timestamp = c.DateTimeOffset(nullable: false, precision: 7),
                        ProgramVersion = c.String(),
                    })
                .PrimaryKey(t => t.Id, clustered: false)
                .Index(t => t.Id, unique: true)
                .Index(t => t.ClusterId, clustered: true);
            
            CreateTable(
                "dbo.ExceptionTelemetryUnits",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ClusterId = c.Int(nullable: false, identity: true),
                        Key = c.String(maxLength: 50),
                        ValueString = c.String(),
                        ValueDouble = c.Double(nullable: false),
                        UnitType = c.Int(nullable: false),
                        ExceptionInfo_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id, clustered: false)
                .ForeignKey("dbo.ExceptionInfoes", t => t.ExceptionInfo_Id)
                .Index(t => t.Id, unique: true)
                .Index(t => t.ClusterId, clustered: true)
                .Index(t => t.ExceptionInfo_Id);
            
            CreateTable(
                "dbo.LogMessages",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Message = c.String(),
                        ProgramVersion = c.String(),
                        Level = c.Int(nullable: false),
                        Sequence = c.String(),
                        ClusterId = c.Int(nullable: false, identity: true),
                        ProgramId = c.Int(nullable: false),
                        UserName = c.String(),
                        Timestamp = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id, clustered: false)
                .Index(t => t.Id, unique: true)
                .Index(t => t.ClusterId, clustered: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExceptionTelemetryUnits", "ExceptionInfo_Id", "dbo.ExceptionInfoes");
            DropForeignKey("dbo.EventTelemetryUnits", "TelemetryDetail_Id", "dbo.EventTelemetryDetails");
            DropForeignKey("dbo.EventTelemetryDetails", "TelemetrySummaryId", "dbo.EventTelemetrySummaries");
            DropForeignKey("dbo.EventTelemetrySummaries", "EventId", "dbo.Events");
            DropForeignKey("dbo.EventTelemetrySummaries", "ClientAppUserId", "dbo.ClientAppUsers");
            DropForeignKey("dbo.ViewTelemetrySummaries", "ViewId", "dbo.Views");
            DropForeignKey("dbo.ViewTelemetryUnits", "TelemetryDetail_Id", "dbo.ViewTelemetryDetails");
            DropForeignKey("dbo.ViewTelemetryDetails", "TelemetrySummaryId", "dbo.ViewTelemetrySummaries");
            DropForeignKey("dbo.ViewTelemetrySummaries", "ClientAppUserId", "dbo.ClientAppUsers");
            DropForeignKey("dbo.Views", "Program_RootObjectId", "dbo.TelemetryRootObjects");
            DropForeignKey("dbo.Events", "Program_RootObjectId", "dbo.TelemetryRootObjects");
            DropIndex("dbo.LogMessages", new[] { "ClusterId" });
            DropIndex("dbo.LogMessages", new[] { "Id" });
            DropIndex("dbo.ExceptionTelemetryUnits", new[] { "ExceptionInfo_Id" });
            DropIndex("dbo.ExceptionTelemetryUnits", new[] { "ClusterId" });
            DropIndex("dbo.ExceptionTelemetryUnits", new[] { "Id" });
            DropIndex("dbo.ExceptionInfoes", new[] { "ClusterId" });
            DropIndex("dbo.ExceptionInfoes", new[] { "Id" });
            DropIndex("dbo.EventTelemetryUnits", new[] { "TelemetryDetail_Id" });
            DropIndex("dbo.EventTelemetryUnits", new[] { "ClusterId" });
            DropIndex("dbo.EventTelemetryUnits", new[] { "Id" });
            DropIndex("dbo.EventTelemetryDetails", new[] { "TelemetrySummaryId" });
            DropIndex("dbo.EventTelemetryDetails", new[] { "ClusterId" });
            DropIndex("dbo.EventTelemetryDetails", new[] { "Id" });
            DropIndex("dbo.EventTelemetrySummaries", new[] { "ClientAppUserId" });
            DropIndex("dbo.EventTelemetrySummaries", new[] { "ClusterId" });
            DropIndex("dbo.EventTelemetrySummaries", new[] { "EventId" });
            DropIndex("dbo.EventTelemetrySummaries", new[] { "Id" });
            DropIndex("dbo.ViewTelemetryUnits", new[] { "TelemetryDetail_Id" });
            DropIndex("dbo.ViewTelemetryUnits", new[] { "ClusterId" });
            DropIndex("dbo.ViewTelemetryUnits", new[] { "Id" });
            DropIndex("dbo.ViewTelemetryDetails", new[] { "TelemetrySummaryId" });
            DropIndex("dbo.ViewTelemetryDetails", new[] { "ClusterId" });
            DropIndex("dbo.ViewTelemetryDetails", new[] { "Id" });
            DropIndex("dbo.ViewTelemetrySummaries", new[] { "ClientAppUserId" });
            DropIndex("dbo.ViewTelemetrySummaries", new[] { "ClusterId" });
            DropIndex("dbo.ViewTelemetrySummaries", new[] { "ViewId" });
            DropIndex("dbo.ViewTelemetrySummaries", new[] { "Id" });
            DropIndex("dbo.Views", new[] { "Program_RootObjectId" });
            DropIndex("dbo.Views", new[] { "ClusterId" });
            DropIndex("dbo.Views", new[] { "Id" });
            DropIndex("dbo.TelemetryRootObjects", new[] { "TelemetryKey" });
            DropIndex("dbo.TelemetryRootObjects", new[] { "ProgramId" });
            DropIndex("dbo.Events", new[] { "Program_RootObjectId" });
            DropIndex("dbo.Events", new[] { "ClusterId" });
            DropIndex("dbo.Events", new[] { "Id" });
            DropTable("dbo.LogMessages");
            DropTable("dbo.ExceptionTelemetryUnits");
            DropTable("dbo.ExceptionInfoes");
            DropTable("dbo.EventTelemetryUnits");
            DropTable("dbo.EventTelemetryDetails");
            DropTable("dbo.EventTelemetrySummaries");
            DropTable("dbo.ViewTelemetryUnits");
            DropTable("dbo.ViewTelemetryDetails");
            DropTable("dbo.ViewTelemetrySummaries");
            DropTable("dbo.Views");
            DropTable("dbo.TelemetryRootObjects");
            DropTable("dbo.Events");
            DropTable("dbo.ClientAppUsers");
        }
    }
}
