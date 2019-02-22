using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Craidd.Models;
using System.Linq;
using System.Security.Claims;

namespace Craidd.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
        }

        public DbSet<Task> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var table = entityType.Relational().TableName;
                if (table.StartsWith("AspNet")) {
                    entityType.Relational().TableName = table.Substring(6);
                }
            };

            // any guid
            const string ADMIN_ID = "a18be9c0-aa65-4af8-bd17-00bd9344e575";
            // any guid, but nothing is against to use the same one
            const string ROLE_ID = ADMIN_ID;
            modelBuilder.Entity<Role>().HasData(new Role
            {
                Id = ROLE_ID,
                Name = "admin",
                NormalizedName = "admin"
            });

            var hasher = new PasswordHasher<User>();
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = ADMIN_ID,
                UserName = "admin",
                NormalizedUserName = "admin".ToUpper(),
                Email = "admin@example.org",
                NormalizedEmail = "admin@example.org".ToUpper(),
                EmailConfirmed = true,
                LockoutEnabled = true,
                PasswordHash = hasher.HashPassword(null, "admin"),
                SecurityStamp = Guid.NewGuid().ToString("D")
            });

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = ROLE_ID,
                UserId = ADMIN_ID
            });

            modelBuilder.Entity<IdentityRoleClaim<string>>().HasData(new IdentityRoleClaim<string>
            {
                Id = 1,
                RoleId = ROLE_ID,
                ClaimType = ClaimTypes.Role,
                ClaimValue = "tasks"
            });
        }
    }
}
