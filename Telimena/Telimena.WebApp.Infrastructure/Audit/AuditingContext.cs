using System.Data.Entity;

namespace MvcAuditLogger
{
    #region Using

    #endregion

    public class AuditingContext : DbContext
    {
        public AuditingContext() : base("name=AuditingDBContext")
        {
            Database.SetInitializer<AuditingContext>(new DropCreateDatabaseIfModelChanges<AuditingContext>());
        }

        public DbSet<Audit> AuditRecords { get; set; }
    }
}