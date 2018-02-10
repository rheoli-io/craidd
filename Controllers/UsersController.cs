using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Craidd.Models;
using Craidd.Services;
using Craidd.Data;

namespace Craidd.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly TasksService _tasks;
        private readonly UsersService _users;

        public UsersController(
            TasksService tasks,
            UsersService users
        ) {
            _tasks = tasks;
            _users = users;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] Craidd.Models.Validators.UserRegister item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            var user = new User { UserName = item.Email, Email = item.Email };
            var result = await _users.UserManager.CreateAsync(user, item.Password);

            return Json(result);
        }
    }
}