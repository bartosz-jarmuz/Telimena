namespace Telimena.WebApi
{
    using System.Collections.Generic;
    using WebApp.Core.Models;

    public class TelimenaDbInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<TelimenaContext>
    {
        protected override void Seed(TelimenaContext context)
        {
            var developers = new List<Developer>
            {
                new Developer {Name = "JimBeam"},
            };

            developers.ForEach(s => context.Developers.Add(s));
            context.SaveChanges();

            var programs = new List<Program>
            {
                new Program{Name="JimmyBeamyApp", DeveloperId = 1},
                new Program{Name="New JimmyBeamyApp", DeveloperId = 1},
            };

            programs.ForEach(s => context.Programs.Add(s));
            context.SaveChanges();
        }
    }
}