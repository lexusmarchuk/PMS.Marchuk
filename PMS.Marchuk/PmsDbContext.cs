using Microsoft.EntityFrameworkCore;
using PMS.Marchuk.Models;

namespace PMS.Marchuk
{
    public class PmsDbContext : DbContext
    {
        public DbSet<Project> Projects { get; set; }

        public DbSet<Task> Tasks { get; set; }

        public PmsDbContext(DbContextOptions<PmsDbContext> options)
            :base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=MARCHUKALEX;Database=PMSMarchuk;Trusted_Connection=True;");
            }
        }
    }
}
