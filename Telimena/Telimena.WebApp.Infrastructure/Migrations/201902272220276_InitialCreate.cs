namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DeveloperTeams",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 75),
                        MainEmail = c.String(),
                        PublicId = c.Guid(nullable: false),
                        MainUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.MainUserId)
                .Index(t => t.Name, unique: true)
                .Index(t => t.PublicId, unique: true)
                .Index(t => t.MainUserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserNumber = c.Int(nullable: false, identity: true),
                        RegisteredDate = c.DateTimeOffset(nullable: false, precision: 7),
                        LastLoginDate = c.DateTimeOffset(precision: 7),
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
                        PublicId = c.Guid(nullable: false),
                        TelemetryKey = c.Guid(nullable: false),
                        RegisteredDate = c.DateTimeOffset(nullable: false, precision: 7),
                        Name = c.String(nullable: false, maxLength: 255),
                        Description = c.String(),
                        DeveloperTeam_Id = c.Int(),
                        Updater_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DeveloperTeams", t => t.DeveloperTeam_Id)
                .ForeignKey("dbo.Updaters", t => t.Updater_Id)
                .Index(t => t.PublicId, unique: true)
                .Index(t => t.TelemetryKey, unique: true)
                .Index(t => t.DeveloperTeam_Id)
                .Index(t => t.Updater_Id);
            
            CreateTable(
                "dbo.ProgramAssemblies",
                c => new
                    {
                        ProgramId = c.Int(nullable: false),
                        Name = c.String(),
                        Extension = c.String(),
                    })
                .PrimaryKey(t => t.ProgramId)
                .ForeignKey("dbo.Programs", t => t.ProgramId)
                .Index(t => t.ProgramId);
            
            CreateTable(
                "dbo.AssemblyVersionInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PublicId = c.Guid(nullable: false),
                        AssemblyVersion = c.String(),
                        FileVersion = c.String(),
                        ReleaseDate = c.DateTimeOffset(precision: 7),
                        ProgramAssemblyId = c.Int(nullable: false),
                        LatestVersionOf_ProgramId = c.Int(),
                        ToolkitData_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProgramAssemblies", t => t.LatestVersionOf_ProgramId)
                .ForeignKey("dbo.ProgramAssemblies", t => t.ProgramAssemblyId, cascadeDelete: true)
                .ForeignKey("dbo.TelimenaToolkitDatas", t => t.ToolkitData_Id)
                .Index(t => t.PublicId, unique: true)
                .Index(t => t.ProgramAssemblyId)
                .Index(t => t.LatestVersionOf_ProgramId)
                .Index(t => t.ToolkitData_Id);
            
            CreateTable(
                "dbo.TelimenaToolkitDatas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Version = c.String(),
                        PublicId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.PublicId, unique: true);
            
            CreateTable(
                "dbo.TelimenaPackageInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        PublicId = c.Guid(nullable: false),
                        Version = c.String(),
                        IsBeta = c.Boolean(nullable: false),
                        IntroducesBreakingChanges = c.Boolean(nullable: false),
                        UploadedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        FileName = c.String(),
                        FileLocation = c.String(),
                        FileSizeBytes = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TelimenaToolkitDatas", t => t.Id)
                .Index(t => t.Id)
                .Index(t => t.PublicId, unique: true);
            
            CreateTable(
                "dbo.Updaters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileName = c.String(nullable: false),
                        InternalName = c.String(nullable: false, maxLength: 255),
                        PublicId = c.Guid(nullable: false),
                        IsPublic = c.Boolean(nullable: false),
                        DeveloperTeam_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DeveloperTeams", t => t.DeveloperTeam_Id)
                .Index(t => t.InternalName, unique: true)
                .Index(t => t.PublicId, unique: true)
                .Index(t => t.DeveloperTeam_Id);
            
            CreateTable(
                "dbo.UpdaterPackageInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PublicId = c.Guid(nullable: false),
                        Version = c.String(),
                        MinimumRequiredToolkitVersion = c.String(),
                        IsBeta = c.Boolean(nullable: false),
                        UpdaterId = c.Int(nullable: false),
                        UploadedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        FileName = c.String(),
                        FileLocation = c.String(),
                        FileSizeBytes = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Updaters", t => t.UpdaterId, cascadeDelete: true)
                .Index(t => t.PublicId, unique: true)
                .Index(t => t.UpdaterId);
            
            CreateTable(
                "dbo.ProgramPackageInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PublicId = c.Guid(nullable: false),
                        ProgramId = c.Int(nullable: false),
                        SupportedToolkitVersion = c.String(),
                        UploadedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        FileName = c.String(),
                        FileLocation = c.String(),
                        FileSizeBytes = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.PublicId, unique: true);
            
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
                        PublicId = c.Guid(nullable: false),
                        ProgramId = c.Int(nullable: false),
                        Version = c.String(),
                        SupportedToolkitVersion = c.String(),
                        IsBeta = c.Boolean(nullable: false),
                        IsStandalone = c.Boolean(nullable: false),
                        ReleaseNotes = c.String(),
                        UploadedDate = c.DateTimeOffset(nullable: false, precision: 7),
                        FileName = c.String(),
                        FileLocation = c.String(),
                        FileSizeBytes = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.PublicId, unique: true);
            
            CreateTable(
                "dbo.TelimenaUserDeveloperAccountAssociations",
                c => new
                    {
                        DeveloperAccount_Id = c.Int(nullable: false),
                        TelimenaUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.DeveloperAccount_Id, t.TelimenaUser_Id })
                .ForeignKey("dbo.DeveloperTeams", t => t.DeveloperAccount_Id, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.TelimenaUser_Id, cascadeDelete: true)
                .Index(t => t.DeveloperAccount_Id)
                .Index(t => t.TelimenaUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Programs", "Updater_Id", "dbo.Updaters");
            DropForeignKey("dbo.UpdaterPackageInfoes", "UpdaterId", "dbo.Updaters");
            DropForeignKey("dbo.Updaters", "DeveloperTeam_Id", "dbo.DeveloperTeams");
            DropForeignKey("dbo.AssemblyVersionInfoes", "ToolkitData_Id", "dbo.TelimenaToolkitDatas");
            DropForeignKey("dbo.TelimenaPackageInfoes", "Id", "dbo.TelimenaToolkitDatas");
            DropForeignKey("dbo.AssemblyVersionInfoes", "ProgramAssemblyId", "dbo.ProgramAssemblies");
            DropForeignKey("dbo.AssemblyVersionInfoes", "LatestVersionOf_ProgramId", "dbo.ProgramAssemblies");
            DropForeignKey("dbo.ProgramAssemblies", "ProgramId", "dbo.Programs");
            DropForeignKey("dbo.Programs", "DeveloperTeam_Id", "dbo.DeveloperTeams");
            DropForeignKey("dbo.DeveloperTeams", "MainUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.TelimenaUserDeveloperAccountAssociations", "TelimenaUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.TelimenaUserDeveloperAccountAssociations", "DeveloperAccount_Id", "dbo.DeveloperTeams");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.TelimenaUserDeveloperAccountAssociations", new[] { "TelimenaUser_Id" });
            DropIndex("dbo.TelimenaUserDeveloperAccountAssociations", new[] { "DeveloperAccount_Id" });
            DropIndex("dbo.ProgramUpdatePackageInfoes", new[] { "PublicId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.ProgramPackageInfoes", new[] { "PublicId" });
            DropIndex("dbo.UpdaterPackageInfoes", new[] { "UpdaterId" });
            DropIndex("dbo.UpdaterPackageInfoes", new[] { "PublicId" });
            DropIndex("dbo.Updaters", new[] { "DeveloperTeam_Id" });
            DropIndex("dbo.Updaters", new[] { "PublicId" });
            DropIndex("dbo.Updaters", new[] { "InternalName" });
            DropIndex("dbo.TelimenaPackageInfoes", new[] { "PublicId" });
            DropIndex("dbo.TelimenaPackageInfoes", new[] { "Id" });
            DropIndex("dbo.TelimenaToolkitDatas", new[] { "PublicId" });
            DropIndex("dbo.AssemblyVersionInfoes", new[] { "ToolkitData_Id" });
            DropIndex("dbo.AssemblyVersionInfoes", new[] { "LatestVersionOf_ProgramId" });
            DropIndex("dbo.AssemblyVersionInfoes", new[] { "ProgramAssemblyId" });
            DropIndex("dbo.AssemblyVersionInfoes", new[] { "PublicId" });
            DropIndex("dbo.ProgramAssemblies", new[] { "ProgramId" });
            DropIndex("dbo.Programs", new[] { "Updater_Id" });
            DropIndex("dbo.Programs", new[] { "DeveloperTeam_Id" });
            DropIndex("dbo.Programs", new[] { "TelemetryKey" });
            DropIndex("dbo.Programs", new[] { "PublicId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.DeveloperTeams", new[] { "MainUserId" });
            DropIndex("dbo.DeveloperTeams", new[] { "PublicId" });
            DropIndex("dbo.DeveloperTeams", new[] { "Name" });
            DropTable("dbo.TelimenaUserDeveloperAccountAssociations");
            DropTable("dbo.ProgramUpdatePackageInfoes");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.ProgramPackageInfoes");
            DropTable("dbo.UpdaterPackageInfoes");
            DropTable("dbo.Updaters");
            DropTable("dbo.TelimenaPackageInfoes");
            DropTable("dbo.TelimenaToolkitDatas");
            DropTable("dbo.AssemblyVersionInfoes");
            DropTable("dbo.ProgramAssemblies");
            DropTable("dbo.Programs");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.DeveloperTeams");
        }
    }
}
