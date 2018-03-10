using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Craidd.Models.Validators;
using Craidd.Models;
using Craidd.Services;

namespace Craidd.Controllers.V1
{
    [ApiVersion( "1.0" )]
    [Route( "api/v{version:apiVersion}/[controller]" )]
  //  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : Controller
    {
        private IConfiguration _config;
        private readonly TasksService _tasks;
        private readonly UsersService _users;

        public UsersController(
            IConfiguration config,
            TasksService tasks,
            UsersService users
        ) {
            _config = config;
            _tasks = tasks;
            _users = users;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var v = new { User = "aweso222me" };  
            return Json(v);
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
        public async Task<IActionResult> Login([FromBody]UserLogin login)
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

        private string BuildToken(User user)
        {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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
