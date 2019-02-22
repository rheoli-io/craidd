using System.Linq;

using Microsoft.AspNetCore.Identity;

using Craidd.Models;

namespace Craidd.Services
{
    public interface IUsersService
    {
        UserManager<User> UserManager { get; }
        SignInManager<User> SignInManager { get; }
        RoleManager<Role> RoleManager { get; }
    }
}