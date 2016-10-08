namespace DotWeb.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Used by entity framework database migration.
    /// </summary>
    internal sealed class Configuration : DbMigrationsConfiguration<DotWeb.DotWebDb>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        /// <summary>
        /// Initializes some data to the generated database.
        /// </summary>
        /// <param name="context">See <see cref="DotWebDb"/>.</param>
        protected override void Seed(DotWeb.DotWebDb context)
        {
#if (DEBUG)
            // BEWARE ! CAUTION !
            // The following code does destructive actions to database. It will delete existing data.
            // Please be sure to comment it in production.
            var modules = context.Modules;
            context.Modules.RemoveRange(modules);
            var groups = context.Groups;
            context.Groups.RemoveRange(groups);
            var apps = context.Apps;
            context.Apps.RemoveRange(apps);
            context.SaveChanges();

            var app1 = new App() { Id = 1, Name = "DotWeb Admin", Description = "Admin app of DotWeb." };
            var app2 = new App() { Id = 2, Name = "DevExpress DevAV", Description = "Sample application using DevExpress DevAVDb schema." };
            var app3 = new App() { Id = 3, Name = "Northwind App", Description = "Sample application using Microsoft Northwind schema." };
            var app4 = new App() { Id = 4, Name = "Mercedes Benz", Description = "Real world application 1." };
            context.Apps.AddRange(new App[] { app1, app2, app3, app4 } );

            context.SaveChanges();
#endif
        }
    }
}
