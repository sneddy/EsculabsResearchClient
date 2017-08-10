namespace Client
{
    using Model;
    using System.Data.Entity;

    public partial class PgContext : DbContext
    {
        public PgContext()
            : base("name=pgContext")
        {
        }

        public virtual DbSet<Patient> Patients { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>()
                .Property(e => e.IIN)
                .IsFixedLength();
        }
    }
}
