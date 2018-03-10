using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Craidd.Models;
using Craidd.Services;
using Craidd.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Craidd.Controllers.V1
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    [ApiVersion( "1.0" )]
    [Route( "api/v{version:apiVersion}/[controller]" )]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TasksController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly TasksService _tasks;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tasks"></param>
        public TasksController(
            AppDbContext context,
            TasksService tasks
        ) {
            _dbContext = context;
            _tasks = tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet, Authorize]
        public IEnumerable<Task> GetAll()
        {
            return _dbContext.Tasks.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetTask")]
        public IActionResult GetById(long id)
        {
            var item = _dbContext.Tasks.FirstOrDefault(t => t.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        /// <summary>
        /// Creates a Task.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Todo
        ///     {
        ///        "id": 1,
        ///        "name": "Item1",
        ///        "isComplete": true
        ///     }
        ///
        /// </remarks>
        /// <param name="item"></param>
        /// <returns>A newly-created Task</returns>
        /// <response code="201">Returns the newly-created item</response>
        /// <response code="400">If the item is null</response>
        [ProducesResponseType(typeof(Task), 201)]
        [ProducesResponseType(typeof(Task), 400)]
        [HttpPost]
        public IActionResult Create([FromBody] Task item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            _dbContext.Tasks.Add(item);
            _dbContext.SaveChanges();

            return CreatedAtRoute("GetTask", new { id = item.Id }, item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public IActionResult Update(long id, [FromBody] Task item)
        {
            if (item == null || item.Id != id)
            {
                return BadRequest();
            }

            var todo = _dbContext.Tasks.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            todo.IsComplete = item.IsComplete;
            todo.Name = item.Name;

            _dbContext.Tasks.Update(todo);
            _dbContext.SaveChanges();
            return new NoContentResult();
        }

        /// <summary>
        /// Deletes a specific Task.
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            var deleted = _tasks.Delete(id);

            if (deleted == false)
            {
                return NotFound();
            }

            return new NoContentResult();
        }
    }
}