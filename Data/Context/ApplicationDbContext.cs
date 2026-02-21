using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Data.Models;

namespace Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<Model2> Model2s { get; set; }
        public DbSet<Model1> Model1s { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Configure the relationship between Model1 and Model2
            builder.Entity<Model1>()
                .HasMany(m => m.Model2s)
                .WithOne()
                .HasForeignKey("Model1Id")
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Model2>();

        }
    }
}
