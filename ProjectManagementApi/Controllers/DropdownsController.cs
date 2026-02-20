using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.DataContext;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DropdownsController : ControllerBase
    {
        private readonly PMDataContext _context;

        public DropdownsController (PMDataContext context)
        {
            _context = context;
        }

        // DTO for returning Project items
        public class ProjectItemDto
        {
            public int ProjectID { get; set; }
            public string ProjectName { get; set; }
        }

        [HttpGet("projects-item")]
        public async Task<ActionResult<IEnumerable<ProjectItemDto>>> GetProjectsItem()
        {
            var result = new List<ProjectItemDto>();

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "ProjectManagement.getProjectsItem";
                command.CommandType = System.Data.CommandType.StoredProcedure;

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new ProjectItemDto
                        {
                            ProjectID = reader.GetInt32(0),
                            ProjectName = reader.GetString(1)
                        });
                    }
                }
            }

            return Ok(result);
        }
    }
}
