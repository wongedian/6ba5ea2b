using System.Data.Entity;

namespace DotWeb
{
    public class DotWebDb : DbContext
    {
        public DotWebDb() : base("DotWebDb") { }

        public DbSet<App> Apps { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Module> Modules { get; set; }

        public DbSet<TableMeta> Tables { get; set; }

        public DbSet<ColumnMeta> Columns { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TableMeta>()
                .HasMany(x => x.Children).WithRequired(y => y.Parent).HasForeignKey(f => f.ParentId).WillCascadeOnDelete(false);

            modelBuilder.Entity<TableMeta>()
                .HasMany(x => x.Parents).WithRequired(y => y.Child).HasForeignKey(f => f.ChildId).WillCascadeOnDelete(false);

            modelBuilder.Entity<TableMeta>()
                .HasOptional(x => x.LookUpDisplayColumn).WithMany().HasForeignKey(f => f.LookUpDisplayColumnId).WillCascadeOnDelete(false);

            modelBuilder.Entity<ColumnMeta>()
                .HasRequired(x => x.Table).WithMany(y => y.Columns).HasForeignKey(f => f.TableId);

            modelBuilder.Entity<ColumnMeta>()
                .HasOptional(x => x.ReferenceTable).WithMany().HasForeignKey(f => f.ReferenceTableId);

            modelBuilder.Entity<Module>()
                .HasRequired(x => x.Group).WithMany(g => g.Modules).HasForeignKey(f => f.GroupId);
        }
    }
}
