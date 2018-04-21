using System.Linq;

using Microsoft.AspNetCore.Identity;

using Craidd.Data;
using Craidd.Models;
using Craidd.Models.Validators;

namespace Craidd.Services
{
    public class UsersService: IUsersService
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UsersService(
            AppDbContext context,
            UserManager<User> userManager,
            SignInManager<User> signInManager
        )
        {
            _dbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public UserManager<User> UserManager => _userManager;
        public SignInManager<User> SignInManager => _signInManager;
    }
}