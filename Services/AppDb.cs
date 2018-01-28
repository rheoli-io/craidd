using Microsoft.EntityFrameworkCore;
using Craidd.Models;

namespace Craidd.Services
{
    public class AppDb : DbContext
    {
        public AppDb(DbContextOptions<AppDb> options): base(options)
        {
        }

        public DbSet<Task> Tasks { get; set; }
    }
}
