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
using Craidd.Helpers;
using Craidd.Models.Validators;
using Craidd.Models;
using Craidd.Services;

namespace Craidd.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
    {
        private IConfiguration _config;
        private readonly TasksService _tasks;
        private readonly IUsersService _users;
        private readonly IHttpContextAccessor _httpContext;
        private IApiResponseHelper _apiResponse;

        private readonly IEmailsService _emails;

        public UsersController(
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            TasksService tasks,
            IUsersService users,
            IApiResponseHelper apiResponse,
            IEmailsService emailsService
        )
        {
            _config = config;
            _tasks = tasks;
            _users = users;
            _httpContext = httpContextAccessor;
            _apiResponse = apiResponse;
            _emails = emailsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _httpContext.CurrentUser();

            var returnData = new Dictionary<string, string>() {
                { "email", user.NormalizedEmail },
                { "username", user.NormalizedUserName },
            };

            return Ok(new { user = returnData });
        }

        [AllowAnonymous]
        [HttpPost(Name = "users_create")]
        public async Task<IActionResult> Store([FromBody] Models.Validators.UserRegister item)
        {
            if (item is null)
            {
                return BadRequest(_apiResponse.AddErrorResponse(code: "InvalidFormData").ErrorReponse);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(_apiResponse.ParseModelStateResponse(ModelState).ErrorReponse);
            }

            var user = new User { UserName = item.UserName, Email = item.Email };
            var result = await _users.UserManager.CreateAsync(user, item.Password);

            if (result.Succeeded == false)
            {
                foreach (var error in result.Errors)
                {
                    _apiResponse.AddErrorResponse(code: error.Code, detail: error.Description);
                }

                return BadRequest(_apiResponse.ErrorReponse);
            }

            var code = await _users.UserManager.GenerateEmailConfirmationTokenAsync(user);

            await this._emails.sendEmailFromTemplateAsync(user.Email, "Registered", "Register", new Dictionary<string, object> {
                {"Name", user.UserName},
                {"Code", code}
            });

            return StatusCode(StatusCodes.Status201Created, new
            {
                data = new { id = user.Id, code = code }
            });
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

            var signInResult = await _users.SignInManager.CheckPasswordSignInAsync(user, login.Password, false);

            if (signInResult.Succeeded == false)
            {
                return Unauthorized();
            }

            return Ok(new { token = await BuildToken(user) });
        }

        [HttpPost("logout")]
        public IActionResult logout()
        {
            return Ok(new { logout = true });
        }

        [AllowAnonymous]
        [HttpPost("confirm")]
        public async Task<IActionResult> confirm([FromBody] Models.Validators.UserRegisterConfirm confirm)
        {
            if (confirm is null)
            {
                return BadRequest(_apiResponse.AddErrorResponse(code: "InvalidFormData").ErrorReponse);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(_apiResponse.ParseModelStateResponse(ModelState).ErrorReponse);
            }

            var user = await _users.UserManager.FindByIdAsync(confirm.Id);
            if (user == null)
            {
                _apiResponse.AddErrorResponse(detail: "Bad Access");

                return BadRequest(_apiResponse.ErrorReponse);
            }

            var result = await _users.UserManager.ConfirmEmailAsync(user, confirm.Code);

            if (result.Succeeded == false)
            {
                foreach (var error in result.Errors)
                {
                    _apiResponse.AddErrorResponse(code: error.Code, detail: error.Description);
                }

                return BadRequest(_apiResponse.ErrorReponse);
            }

            return StatusCode(StatusCodes.Status201Created, new
            {
                data = new { id = user.Id }
            });
        }

        private async Task<string> BuildToken(User user)
        {
            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            };

            var userRoles = await _users.UserManager.GetRolesAsync(user);

            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim("role", userRole));

                var role = await _users.RoleManager.FindByNameAsync(userRole);

                if (role == null)
                {
                    continue;
                }

                var roleClaims = await _users.RoleManager.GetClaimsAsync(role);

                foreach (Claim roleClaim in roleClaims)
                {
                    claims.Add(roleClaim);
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
