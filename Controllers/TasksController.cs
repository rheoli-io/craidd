using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

using Craidd.Models;
using Craidd.Services;

namespace Craidd.Controllers
{
    [Route("api/[controller]")]
    public class TasksController : Controller
    {
        private readonly AppDbContext _db_context;

        public TasksController(AppDbContext context)
        {
            _db_context = context;
        }

        [HttpGet]
        public IEnumerable<Task> GetAll()
        {
            return _db_context.Tasks.ToList();
        }

        [HttpGet("{id}", Name = "GetTask")]
        public IActionResult GetById(long id)
        {
            var item = _db_context.Tasks.FirstOrDefault(t => t.Id == id);
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

            _db_context.Tasks.Add(item);
            _db_context.SaveChanges();

            return CreatedAtRoute("GetTask", new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public IActionResult Update(long id, [FromBody] Task item)
        {
            if (item == null || item.Id != id)
            {
                return BadRequest();
            }

            var todo = _db_context.Tasks.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            todo.IsComplete = item.IsComplete;
            todo.Name = item.Name;

            _db_context.Tasks.Update(todo);
            _db_context.SaveChanges();
            return new NoContentResult();
        }

        /// <summary>
        /// Deletes a specific Task.
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            var todo = _db_context.Tasks.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            _db_context.Tasks.Remove(todo);
            _db_context.SaveChanges();
            return new NoContentResult();
        }
    }
}