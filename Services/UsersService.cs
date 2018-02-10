using System.Linq;

using Microsoft.AspNetCore.Identity;

using Craidd.Data;
using Craidd.Models;
using Craidd.Models.Validators;

namespace Craidd.Services
{
    public class UsersService
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        public UsersService(
            AppDbContext context,
            UserManager<User> userManager
        )
        {
            _dbContext = context;
            _userManager = userManager;
        }

        public UserManager<User> UserManager => _userManager;
    }
}