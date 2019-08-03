using System.Linq;

using Microsoft.AspNetCore.Identity;

using Craidd.Data;
using Craidd.Models;

namespace Craidd.Services
{
    public class UsersService: IUsersService
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;

        public UsersService(
            AppDbContext context,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<Role> roleManager
        )
        {
            _dbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public UserManager<User> UserManager => _userManager;
        public SignInManager<User> SignInManager => _signInManager;
        public RoleManager<Role> RoleManager => _roleManager;
    }
}