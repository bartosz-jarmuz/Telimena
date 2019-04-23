using System.Data.Entity;

namespace Telimena.Tests
{
    internal sealed class DropCreateDatabaseAlwaysWithConnectionClose : DropCreateDatabaseAlways<DbContext>
    {
        public override void InitializeDatabase(DbContext context)
        {
            if (context.Database.Exists())
                context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", (object)context.Database.Connection.Database));
            base.InitializeDatabase(context);
        }
    }
}