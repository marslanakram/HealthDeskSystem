using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HealthDesk.Infrastructure.Data
{
    public class DesignTimeAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Use your normal connection string
            optionsBuilder.UseSqlServer("Server=DESKTOP-9OL9JKM\\SQLEXPRESS;Database=HealthDeskSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
