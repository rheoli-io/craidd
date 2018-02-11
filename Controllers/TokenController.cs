using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Craidd.Models.Validators;
using Craidd.Models;
using Craidd.Services;

namespace Craidd.Controllers
{
    [Route("api/[controller]")]
    public class TokenController : Controller
    {
        private IConfiguration _config;
        private readonly UsersService _users;

        public TokenController(
            IConfiguration config,
            UsersService users
        )
        {
            _config = config;
            _users = users;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateToken([FromBody]UserLogin login)
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

            var tokenString = BuildToken(user);

            return Ok(tokenString);
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