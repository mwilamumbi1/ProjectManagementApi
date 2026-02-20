using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.DataContext;
using System.Threading.Tasks;

namespace ProjectManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountsController : ControllerBase
    {
        private readonly PMDataContext _context;

        public CountsController(PMDataContext context)
        {
            _context = context;
        }

        // 📌 Helper to execute scalar count queries
        private async Task<int> ExecuteCountQuery(string sql)
        {
            int count = 0;
            await using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.CommandType = System.Data.CommandType.Text;

            await _context.Database.OpenConnectionAsync();
            var result = await command.ExecuteScalarAsync();
            if (result != null && int.TryParse(result.ToString(), out var parsed))
            {
                count = parsed;
            }
            return count;
        }

        // 📌 Count Tasks
        [HttpGet("tasks")]
        public async Task<IActionResult> CountTasks()
        {
            var total = await ExecuteCountQuery("EXEC ProjectManagement.CountTasks");
            return Ok(new { totalTasks = total });
        }

        // 📌 Count Projects
        [HttpGet("projects")]
        public async Task<IActionResult> CountProjects()
        {
            var total = await ExecuteCountQuery("EXEC ProjectManagement.CountProjects");
            return Ok(new { totalProjects = total });
        }

        // 📌 Count Issues
        [HttpGet("issues")]
        public async Task<IActionResult> CountIssues()
        {
            var total = await ExecuteCountQuery("EXEC ProjectManagement.CountIssues");
            return Ok(new { totalIssues = total });
        }

        // 📌 Count Employees
        [HttpGet("employees")]
        public async Task<IActionResult> CountEmployees()
        {
            var total = await ExecuteCountQuery("EXEC ProjectManagement.CountEmployees");
            return Ok(new { totalEmployees = total });
        }

        // 📌 Count Clients
        [HttpGet("clients")]
        public async Task<IActionResult> CountClients()
        {
            var total = await ExecuteCountQuery("EXEC ProjectManagement.CountClients");
            return Ok(new { totalClients = total });
        }

        // 📌 Count Bills
        [HttpGet("bills")]
        public async Task<IActionResult> CountBills()
        {
            var total = await ExecuteCountQuery("EXEC ProjectManagement.CountBills");
            return Ok(new { totalBills = total });
        }
    }
}
