using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Context
{
    public class DbContextInitializer
    {
        public static ApplicationDbContext Create(string connectionString)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(connectionString).Options;

            return new ApplicationDbContext(options);
        }
    }
}