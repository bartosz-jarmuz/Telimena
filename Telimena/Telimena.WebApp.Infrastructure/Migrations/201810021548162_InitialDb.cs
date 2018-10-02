namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialDb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClientAppUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RegisteredDate = c.DateTime(nullable: false),
                        UserName = c.String(),
                        Email = c.String(),
                        MachineName = c.String(),
                        IpAddress = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DeveloperAccounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        MainEmail = c.String(),
                        MainUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.MainUserId)
                .Index(t => t.MainUserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserNumber = c.Int(nullable: false, identity: true),
                        RegisteredDate = c.DateTime(nullable: false),
                        LastLoginDate = c.DateTime(),
                        DisplayName = c.String(),
                        IsActivated = c.Boolean(nullable: false),
                        MustChangePassword = c.Boolean(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Programs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RegisteredDate = c.DateTime(nullable: false),
                        Name = c.String(nullable: false, maxLength: 450),
                        Description = c.String(),
                        DeveloperAccount_Id = c.Int(),
                        Updater_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DeveloperAccounts", t => t.DeveloperAccount_Id)
                .ForeignKey("dbo.Updaters", t => t.Updater_Id)
                .Index(t => t.Name, unique: true)
                .Index(t => t.DeveloperAccount_Id)
                .Index(t => t.Updater_Id);
            
            CreateTable(
                "dbo.Functions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ProgramId = c.Int(nullable: false),
                        RegisteredDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Programs", t => t.ProgramId, cascadeDelete: true)
                .Index(t => t.ProgramId);
            
            CreateTable(
                "dbo.FunctionUsageSummaries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FunctionId = c.Int(nullable: false),
                        SummaryCount = c.Int(nullable: false),
                        LastUsageDateTime = c.DateTime(nullable: false),
                        ClientAppUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ClientAppUsers", t => t.ClientAppUserId)
                .ForeignKey("dbo.Functions", t => t.FunctionId, cascadeDelete: true)
                .Index(t => t.FunctionId)
                .Index(t => t.ClientAppUserId);
            
            CreateTable(
                "dbo.FunctionUsageDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateTime = c.DateTime(nullable: false),
                        UsageSummaryId = c.Int(nullable: false),
                        AssemblyVersionId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AssemblyVersions", t => t.AssemblyVersionId)
                .ForeignKey("dbo.FunctionUsageSummaries", t => t.UsageSummaryId, cascadeDelete: true)
                .Index(t => t.UsageSummaryId)
                .Index(t => t.AssemblyVersionId);
            
            CreateTable(
                "dbo.AssemblyVersions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Version = c.String(),
                        ReleaseDate = c.DateTime(),
                        ProgramAssemblyId = c.Int(nullable: false),
                        LatestVersionOf_Id = c.Int(),
                        ToolkitData_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProgramAssemblies", t => t.LatestVersionOf_Id)
                .ForeignKey("dbo.ProgramAssemblies", t => t.ProgramAssemblyId, cascadeDelete: true)
                .ForeignKey("dbo.TelimenaToolkitDatas", t => t.ToolkitData_Id)
                .Index(t => t.ProgramAssemblyId)
                .Index(t => t.LatestVersionOf_Id)
                .Index(t => t.ToolkitData_Id);
            
            CreateTable(
                "dbo.ProgramAssemblies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProgramId = c.Int(nullable: false),
                        Name = c.String(),
                        Extension = c.String(),
                        Product = c.String(),
                        Trademark = c.String(),
                        Description = c.String(),
                        Copyright = c.String(),
                        Title = c.String(),
                        Company = c.String(),
                        FullName = c.String(),
                        PrimaryOf_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Programs", t => t.ProgramId, cascadeDelete: true)
                .ForeignKey("dbo.Programs", t => t.PrimaryOf_Id)
                .Index(t => t.ProgramId)
                .Index(t => t.PrimaryOf_Id);
            
            CreateTable(
                "dbo.TelimenaToolkitDatas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Version = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TelimenaPackageInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Version = c.String(),
                        IsBeta = c.Boolean(nullable: false),
                        IntroducesBreakingChanges = c.Boolean(nullable: false),
                        UploadedDate = c.DateTime(nullable: false),
                        FileName = c.String(),
                        FileLocation = c.String(),
                        FileSizeBytes = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TelimenaToolkitDatas", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.FunctionCustomUsageDatas",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Data = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FunctionUsageDetails", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Updaters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileName = c.String(nullable: false),
                        InternalName = c.String(nullable: false, maxLength: 255),
                        DeveloperAccount_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DeveloperAccounts", t => t.DeveloperAccount_Id)
                .Index(t => t.InternalName, unique: true)
                .Index(t => t.DeveloperAccount_Id);
            
            CreateTable(
                "dbo.UpdaterPackageInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Version = c.String(),
                        MinimumRequiredToolkitVersion = c.String(),
                        IsBeta = c.Boolean(nullable: false),
                        UpdaterId = c.Int(nullable: false),
                        UploadedDate = c.DateTime(nullable: false),
                        FileName = c.String(),
                        FileLocation = c.String(),
                        FileSizeBytes = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Updaters", t => t.UpdaterId, cascadeDelete: true)
                .Index(t => t.UpdaterId);
            
            CreateTable(
                "dbo.ProgramUsageSummaries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SummaryCount = c.Int(nullable: false),
                        ProgramId = c.Int(nullable: false),
                        LastUsageDateTime = c.DateTime(nullable: false),
                        ClientAppUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ClientAppUsers", t => t.ClientAppUserId)
                .ForeignKey("dbo.Programs", t => t.ProgramId, cascadeDelete: true)
                .Index(t => t.ProgramId)
                .Index(t => t.ClientAppUserId);
            
            CreateTable(
                "dbo.ProgramUsageDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateTime = c.DateTime(nullable: false),
                        UsageSummaryId = c.Int(nullable: false),
                        AssemblyVersionId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AssemblyVersions", t => t.AssemblyVersionId)
                .ForeignKey("dbo.ProgramUsageSummaries", t => t.UsageSummaryId, cascadeDelete: true)
                .Index(t => t.UsageSummaryId)
                .Index(t => t.AssemblyVersionId);
            
            CreateTable(
                "dbo.ProgramCustomUsageDatas",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Data = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProgramUsageDetails", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.ProgramPackageInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProgramId = c.Int(nullable: false),
                        SupportedToolkitVersion = c.String(),
                        UploadedDate = c.DateTime(nullable: false),
                        FileName = c.String(),
                        FileLocation = c.String(),
                        FileSizeBytes = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.ProgramUpdatePackageInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProgramId = c.Int(nullable: false),
                        Version = c.String(),
                        SupportedToolkitVersion = c.String(),
                        IsBeta = c.Boolean(nullable: false),
                        IsStandalone = c.Boolean(nullable: false),
                        UploadedDate = c.DateTime(nullable: false),
                        FileName = c.String(),
                        FileLocation = c.String(),
                        FileSizeBytes = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TelimenaUserDeveloperAccountAssociations",
                c => new
                    {
                        DeveloperAccount_Id = c.Int(nullable: false),
                        TelimenaUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.DeveloperAccount_Id, t.TelimenaUser_Id })
                .ForeignKey("dbo.DeveloperAccounts", t => t.DeveloperAccount_Id, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.TelimenaUser_Id, cascadeDelete: true)
                .Index(t => t.DeveloperAccount_Id)
                .Index(t => t.TelimenaUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.ProgramUsageDetails", "UsageSummaryId", "dbo.ProgramUsageSummaries");
            DropForeignKey("dbo.ProgramCustomUsageDatas", "Id", "dbo.ProgramUsageDetails");
            DropForeignKey("dbo.ProgramUsageDetails", "AssemblyVersionId", "dbo.AssemblyVersions");
            DropForeignKey("dbo.ProgramUsageSummaries", "ProgramId", "dbo.Programs");
            DropForeignKey("dbo.ProgramUsageSummaries", "ClientAppUserId", "dbo.ClientAppUsers");
            DropForeignKey("dbo.Programs", "Updater_Id", "dbo.Updaters");
            DropForeignKey("dbo.UpdaterPackageInfoes", "UpdaterId", "dbo.Updaters");
            DropForeignKey("dbo.Updaters", "DeveloperAccount_Id", "dbo.DeveloperAccounts");
            DropForeignKey("dbo.ProgramAssemblies", "PrimaryOf_Id", "dbo.Programs");
            DropForeignKey("dbo.FunctionUsageDetails", "UsageSummaryId", "dbo.FunctionUsageSummaries");
            DropForeignKey("dbo.FunctionCustomUsageDatas", "Id", "dbo.FunctionUsageDetails");
            DropForeignKey("dbo.FunctionUsageDetails", "AssemblyVersionId", "dbo.AssemblyVersions");
            DropForeignKey("dbo.AssemblyVersions", "ToolkitData_Id", "dbo.TelimenaToolkitDatas");
            DropForeignKey("dbo.TelimenaPackageInfoes", "Id", "dbo.TelimenaToolkitDatas");
            DropForeignKey("dbo.AssemblyVersions", "ProgramAssemblyId", "dbo.ProgramAssemblies");
            DropForeignKey("dbo.ProgramAssemblies", "ProgramId", "dbo.Programs");
            DropForeignKey("dbo.AssemblyVersions", "LatestVersionOf_Id", "dbo.ProgramAssemblies");
            DropForeignKey("dbo.FunctionUsageSummaries", "FunctionId", "dbo.Functions");
            DropForeignKey("dbo.FunctionUsageSummaries", "ClientAppUserId", "dbo.ClientAppUsers");
            DropForeignKey("dbo.Functions", "ProgramId", "dbo.Programs");
            DropForeignKey("dbo.Programs", "DeveloperAccount_Id", "dbo.DeveloperAccounts");
            DropForeignKey("dbo.DeveloperAccounts", "MainUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.TelimenaUserDeveloperAccountAssociations", "TelimenaUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.TelimenaUserDeveloperAccountAssociations", "DeveloperAccount_Id", "dbo.DeveloperAccounts");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.TelimenaUserDeveloperAccountAssociations", new[] { "TelimenaUser_Id" });
            DropIndex("dbo.TelimenaUserDeveloperAccountAssociations", new[] { "DeveloperAccount_Id" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.ProgramCustomUsageDatas", new[] { "Id" });
            DropIndex("dbo.ProgramUsageDetails", new[] { "AssemblyVersionId" });
            DropIndex("dbo.ProgramUsageDetails", new[] { "UsageSummaryId" });
            DropIndex("dbo.ProgramUsageSummaries", new[] { "ClientAppUserId" });
            DropIndex("dbo.ProgramUsageSummaries", new[] { "ProgramId" });
            DropIndex("dbo.UpdaterPackageInfoes", new[] { "UpdaterId" });
            DropIndex("dbo.Updaters", new[] { "DeveloperAccount_Id" });
            DropIndex("dbo.Updaters", new[] { "InternalName" });
            DropIndex("dbo.FunctionCustomUsageDatas", new[] { "Id" });
            DropIndex("dbo.TelimenaPackageInfoes", new[] { "Id" });
            DropIndex("dbo.ProgramAssemblies", new[] { "PrimaryOf_Id" });
            DropIndex("dbo.ProgramAssemblies", new[] { "ProgramId" });
            DropIndex("dbo.AssemblyVersions", new[] { "ToolkitData_Id" });
            DropIndex("dbo.AssemblyVersions", new[] { "LatestVersionOf_Id" });
            DropIndex("dbo.AssemblyVersions", new[] { "ProgramAssemblyId" });
            DropIndex("dbo.FunctionUsageDetails", new[] { "AssemblyVersionId" });
            DropIndex("dbo.FunctionUsageDetails", new[] { "UsageSummaryId" });
            DropIndex("dbo.FunctionUsageSummaries", new[] { "ClientAppUserId" });
            DropIndex("dbo.FunctionUsageSummaries", new[] { "FunctionId" });
            DropIndex("dbo.Functions", new[] { "ProgramId" });
            DropIndex("dbo.Programs", new[] { "Updater_Id" });
            DropIndex("dbo.Programs", new[] { "DeveloperAccount_Id" });
            DropIndex("dbo.Programs", new[] { "Name" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.DeveloperAccounts", new[] { "MainUserId" });
            DropTable("dbo.TelimenaUserDeveloperAccountAssociations");
            DropTable("dbo.ProgramUpdatePackageInfoes");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.ProgramPackageInfoes");
            DropTable("dbo.ProgramCustomUsageDatas");
            DropTable("dbo.ProgramUsageDetails");
            DropTable("dbo.ProgramUsageSummaries");
            DropTable("dbo.UpdaterPackageInfoes");
            DropTable("dbo.Updaters");
            DropTable("dbo.FunctionCustomUsageDatas");
            DropTable("dbo.TelimenaPackageInfoes");
            DropTable("dbo.TelimenaToolkitDatas");
            DropTable("dbo.ProgramAssemblies");
            DropTable("dbo.AssemblyVersions");
            DropTable("dbo.FunctionUsageDetails");
            DropTable("dbo.FunctionUsageSummaries");
            DropTable("dbo.Functions");
            DropTable("dbo.Programs");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.DeveloperAccounts");
            DropTable("dbo.ClientAppUsers");
        }
    }
}
