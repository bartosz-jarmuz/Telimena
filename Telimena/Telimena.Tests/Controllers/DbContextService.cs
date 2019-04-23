using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Telimena.Tests
{
    internal static class DbContextService
    {
        public static void DropDatabase(DbContext context)
        {
            DbContextService.ResetChangeTracker(context);
            context.Database.Delete();
            context.Dispose();
        }

        public static void ResetChangeTracker(DbContext context)
        {
            foreach (DbEntityEntry dbEntityEntry in context.ChangeTracker.Entries().Where<DbEntityEntry>((Func<DbEntityEntry, bool>)(e =>
            {
                if (e.State != EntityState.Added && e.State != EntityState.Modified)
                    return e.State == EntityState.Deleted;
                return true;
            })))
                context.Entry(dbEntityEntry.Entity).State = EntityState.Detached;
        }

        public static T CreateInstance<T>(string connectionString)
        {
            try
            {
                return (T)Activator.CreateInstance(typeof(T), (object)connectionString);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Your DB context has to have a constructor which allows setting a connection string", ex);
            }
        }
    }
}