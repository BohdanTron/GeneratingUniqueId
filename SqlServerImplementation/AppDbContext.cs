using Microsoft.EntityFrameworkCore;

namespace SqlServerImplementation
{
    public class AppDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Initial Catalog=OrderMaster;Integrated Security = true;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<long>("MySequence", schema: "dbo")
                .StartsAt(1)
                .IncrementsBy(1);
        }
    }
}
