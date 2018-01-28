using Microsoft.EntityFrameworkCore;
using Craidd.Models;

namespace Craidd.Services
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
        }

        public DbSet<Task> Tasks { get; set; }
    }
}
