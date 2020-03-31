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
    }
}
