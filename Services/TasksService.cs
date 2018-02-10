using System.Linq;

using Craidd.Data;

namespace Craidd.Services
{
    public class TasksService
    {
        private readonly AppDbContext _dbContext;
        public TasksService(AppDbContext context)
        {
            _dbContext = context;
        }

        public bool Delete(long id)
        {
            var todo = _dbContext.Tasks.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return false;
            }

            _dbContext.Tasks.Remove(todo);
            _dbContext.SaveChanges();

            return true;
        }
    }
}