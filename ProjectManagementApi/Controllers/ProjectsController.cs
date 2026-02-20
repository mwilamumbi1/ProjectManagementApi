using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.DataContext;
using ProjectManagementApi.DTOs;
using System.Data;

namespace ProjectManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly PMDataContext _context;
        private readonly string _connectionString;

        public ProjectsController(PMDataContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost("add-project")]
        public async Task<ActionResult<ApiResponse<object>>> CreateProject([FromBody] CreateProjectDto dto)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new
            {
                dto.ProjectName,
                dto.Description,
                dto.StartDate,
                dto.EndDate,
                dto.UserID
            };

            var result = await connection.QueryFirstOrDefaultAsync<ApiResponse<object>>(
                "[ProjectManagement].[AddProject]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result?.Success == true ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}GetProjectById")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var projectData = await connection.QueryMultipleAsync(
                "[ProjectManagement].[GetProjectById]",
                new { ProjectID = id },
                commandType: CommandType.StoredProcedure
            );

            var project = await projectData.ReadFirstOrDefaultAsync<ProjectDto>();
            var apiResponse = await projectData.ReadFirstOrDefaultAsync<ApiResponse<object>>();

            if (apiResponse?.MessageType == "WARNING")
            {
                return NotFound(apiResponse);
            }
            if (apiResponse?.MessageType == "ERROR")
            {
                return BadRequest(apiResponse);
            }

            return Ok(project);
        }

        [HttpGet("get-all-projects")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAllProjects()
        {
            using var connection = new SqlConnection(_connectionString);

            var multi = await connection.QueryMultipleAsync(
                "[ProjectManagement].[GetAllProjects]",
                commandType: CommandType.StoredProcedure
            );

            var projects = await multi.ReadAsync<ProjectDto>();
            var apiResponse = await multi.ReadFirstOrDefaultAsync<ApiResponse<object>>();

            if (apiResponse?.Success == false)
            {
                return BadRequest(apiResponse);
            }

            return Ok(projects);
        }

        [HttpPut("{id}UpdateProject")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateProject(int id, [FromBody] UpdateProjectDto dto)
        {
            dto.ProjectID = id;

            using var connection = new SqlConnection(_connectionString);

            var result = await connection.QueryFirstOrDefaultAsync<ApiResponse<object>>(
                "[ProjectManagement].[UpdateProject]",
                dto,
                commandType: CommandType.StoredProcedure
            );

            return result?.Success == true ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}DeleteProject")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteProject(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new { ProjectID = id };

            var result = await connection.QueryFirstOrDefaultAsync<ApiResponse<object>>(
                "[ProjectManagement].[DeleteProject]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result?.Success == true ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}/dependencies")]
        public async Task<ActionResult<ProjectDependenciesDto>> CheckProjectDependencies(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var result = await connection.QueryFirstOrDefaultAsync<ProjectDependenciesDto>(
                "[ProjectManagement].[CheckProjectDependencies]",
                new { ProjectID = id },
                commandType: CommandType.StoredProcedure
            );

            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("{id}/summary")]
        public async Task<ActionResult<ProjectSummaryDetailsDto>> GetProjectSummary(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var result = await connection.QueryFirstOrDefaultAsync<ProjectSummaryDetailsDto>(
                "[ProjectManagement].[GetProjectDetailsByID]",
                new { ProjectID = id },
                commandType: CommandType.StoredProcedure
            );

            if (result?.Success == false)
            {
                return NotFound(result);
            }

            // Deserialize JSON columns into their respective lists
            if (!string.IsNullOrEmpty(result.ClientsList))
            {
                result.Clients = System.Text.Json.JsonSerializer.Deserialize<List<ClientSummaryDto>>(result.ClientsList);
            }

            if (!string.IsNullOrEmpty(result.TasksList))
            {
                result.Tasks = System.Text.Json.JsonSerializer.Deserialize<List<TaskSummaryDto>>(result.TasksList);
            }

            if (!string.IsNullOrEmpty(result.IssuesList))
            {
                result.Issues = System.Text.Json.JsonSerializer.Deserialize<List<IssueSummaryDto>>(result.IssuesList);
            }

            if (!string.IsNullOrEmpty(result.ResourcesList))
            {
                result.Resources = System.Text.Json.JsonSerializer.Deserialize<List<ResourceSummaryDto>>(result.ResourcesList);
            }

            if (!string.IsNullOrEmpty(result.BillingsList))
            {
                result.Billings = System.Text.Json.JsonSerializer.Deserialize<List<BillingSummaryItemDto>>(result.BillingsList);
            }

            return Ok(result);
        }


        [HttpPut("CompleteProject/{projectId}")]
        public async Task<IActionResult> CompleteProject(int projectId)
        {
            try
            {
                var projectIdParam = new SqlParameter("@ProjectID", projectId);

                var outputParam = new SqlParameter
                {
                    ParameterName = "@OutputMessage",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 500,
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC ProjectManagement.ChangeProjectStatusToCompleted @ProjectID, @OutputMessage OUTPUT",
                    projectIdParam, outputParam
                );

                return Ok(new
                {
                    success = !outputParam.Value.ToString().StartsWith("Cannot"),
                    message = outputParam.Value.ToString()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while completing the project.",
                    error = ex.Message
                });
            }
        }

    }
}