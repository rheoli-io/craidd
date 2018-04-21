using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Craidd.Extensions;
using Craidd.Models.Validators;
using Craidd.Models;
using Craidd.Services;

namespace Craidd.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : Controller
    {
        private IConfiguration _config;
        private readonly TasksService _tasks;
        private readonly IUsersService _users;

        private readonly IHttpContextAccessor _httpContext;

        public UsersController(
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            TasksService tasks,
            IUsersService users
        )
        {
            _config = config;
            _tasks = tasks;
            _users = users;
            _httpContext = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _httpContext.CurrentUser();

            var returnData = new Dictionary<string, string>() {
                { "email", user.NormalizedEmail },
                { "username", user.NormalizedUserName },
            };

            return Json(new { user = returnData });
        }

        [HttpPost]
        public async Task<IActionResult> Store([FromBody] Models.Validators.UserRegister item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            var user = new User { UserName = item.Email, Email = item.Email };
            var result = await _users.UserManager.CreateAsync(user, item.Password);

            return Json(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Models.Validators.UserLogin login)
        {
            var user = await _users.UserManager.FindByEmailAsync(login.Email);

            if (user == null)
            {
                return Unauthorized();
            }

            var validPasword = await _users.UserManager.CheckPasswordAsync(user, login.Password);

            if (validPasword == false)
            {
                return Unauthorized();
            }

            return Json(new { token = BuildToken(user) });
        }

        [HttpPost("logout")]
        public IActionResult logout()
        {
            return Json(new { logout = true });
        }

        private string BuildToken(User user)
        {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
