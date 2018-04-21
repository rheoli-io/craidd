using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

using Craidd.Models;
using Craidd.Services;

namespace Craidd.Extensions {
    public static class IHttpContextAccessorExtension
    {
        public static async Task<User> CurrentUser(this IHttpContextAccessor httpContextAccessor)
        {
            IUsersService users = httpContextAccessor.HttpContext.RequestServices.GetService(typeof(IUsersService)) as IUsersService;
            return await users.UserManager.GetUserAsync(httpContextAccessor.HttpContext.User);
        }
    }
}