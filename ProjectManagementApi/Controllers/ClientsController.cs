using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;
using ProjectManagementApi.DataContext;
using System.Text.Json;

namespace ProjectManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly PMDataContext _context;

        public ClientController(PMDataContext context)
        {
            _context = context;
        }

        [HttpGet("clients")]
        public async Task<ActionResult<List<ClientDto>>> GetClients()
        {
            var clients = await _context.ClientDtos
                .FromSqlRaw("EXEC [ProjectManagement].[GetClients]")
                .ToListAsync();

            return Ok(clients);
        }
        [HttpGet("dashboard/{clientId}")]
        public async Task<ActionResult<ClientDashboardDto>> GetClientDashboard(int clientId)
        {
            var dashboard = new ClientDashboardDto();

            using var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "[ProjectManagement].[GetClientDashboard]";
            cmd.CommandType = CommandType.StoredProcedure;

            var param = cmd.CreateParameter();
            param.ParameterName = "@ClientID";
            param.Value = clientId;
            cmd.Parameters.Add(param);

            using var reader = await cmd.ExecuteReaderAsync();

            // 1️⃣ Projects List
            while (await reader.ReadAsync())
            {
                dashboard.Projects.Add(new DashboardProjectDto
                {
                    ClientProjectID = reader.IsDBNull(reader.GetOrdinal("ClientProjectID"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("ClientProjectID")),
                    ProjectID = reader.IsDBNull(reader.GetOrdinal("ProjectID"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("ProjectID")),
                    ProjectName = reader.IsDBNull(reader.GetOrdinal("ProjectName"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("ProjectName")),
                    ProjectDescription = reader.IsDBNull(reader.GetOrdinal("ProjectDescription"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("ProjectDescription")),
                    StartDate = reader.IsDBNull(reader.GetOrdinal("StartDate"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("StartDate")),
                    EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("EndDate")),
                    StatusID = reader.IsDBNull(reader.GetOrdinal("StatusID"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("StatusID")),
                    ProjectStatus = reader.IsDBNull(reader.GetOrdinal("ProjectStatus"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("ProjectStatus"))
                });
            }

            // 2️⃣ Project Counts
            await reader.NextResultAsync();
            if (await reader.ReadAsync())
            {
                dashboard.ProjectCounts = new DashboardProjectCountsDto
                {
                    TotalProjects = reader.IsDBNull(reader.GetOrdinal("TotalProjects"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("TotalProjects")),
                    PlannedProjects = reader.IsDBNull(reader.GetOrdinal("PlannedProjects"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("PlannedProjects")),
                    InProgressProjects = reader.IsDBNull(reader.GetOrdinal("InProgressProjects"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("InProgressProjects")),
                    CompletedProjects = reader.IsDBNull(reader.GetOrdinal("CompletedProjects"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("CompletedProjects")),
                    OnHoldProjects = reader.IsDBNull(reader.GetOrdinal("OnHoldProjects"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("OnHoldProjects"))
                };
            }

            // 3️⃣ Issues List
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
            {
                dashboard.Issues.Add(new DashboardIssueDto
                {
                    IssueID = reader.IsDBNull(reader.GetOrdinal("IssueID"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("IssueID")),
                    ClientProjectID = reader.IsDBNull(reader.GetOrdinal("ClientProjectID"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("ClientProjectID")),
                    IssueTitle = reader.IsDBNull(reader.GetOrdinal("IssueTitle"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("IssueTitle")),
                    IssueDescription = reader.IsDBNull(reader.GetOrdinal("IssueDescription"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("IssueDescription")),
                    Status = reader.IsDBNull(reader.GetOrdinal("Status"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("Status")),
                    CreatedDate = reader.IsDBNull(reader.GetOrdinal("CreatedDate"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                    ResolvedDate = reader.IsDBNull(reader.GetOrdinal("ResolvedDate"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("ResolvedDate")),
                    ResolvedByName = reader.IsDBNull(reader.GetOrdinal("ResolvedByName"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("ResolvedByName"))
                });
            }

            // 4️⃣ Issue Counts
            await reader.NextResultAsync();
            if (await reader.ReadAsync())
            {
                dashboard.IssueCounts = new DashboardIssueCountsDto
                {
                    TotalIssues = reader.IsDBNull(reader.GetOrdinal("TotalIssues"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("TotalIssues")),
                    OpenIssues = reader.IsDBNull(reader.GetOrdinal("OpenIssues"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("OpenIssues")),
                    ResolvedIssues = reader.IsDBNull(reader.GetOrdinal("ResolvedIssues"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("ResolvedIssues"))
                };
            }

            return Ok(dashboard);
        }



        [HttpGet("client-projects")]
        public async Task<ActionResult<List<ClientProjectDto>>> GetClientProjects()
        {
            var clientProjects = await _context.ClientProjectDtos
                .FromSqlRaw("EXEC [ProjectManagement].[GetClientProjects]")
                .ToListAsync();

            return Ok(clientProjects);
        }

        [HttpPost("insert-client")]
        public async Task<ActionResult<ApiResponse>> InsertClient([FromBody] InsertClientRequest request)
        {
            var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 4000) { Direction = ParameterDirection.Output };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [ProjectManagement].[InsertClient] @ClientName, @ContactEmail, @ContactPhone, @Success OUTPUT, @Message OUTPUT",
                new SqlParameter("@ClientName", request.ClientName),
                new SqlParameter("@ContactEmail", (object)request.ContactEmail ?? DBNull.Value),
                new SqlParameter("@ContactPhone", (object)request.ContactPhone ?? DBNull.Value),
                successParam,
                messageParam
            );

            var success = (bool)successParam.Value;
            var message = messageParam.Value?.ToString();

            return Ok(new ApiResponse { Success = success, Message = message });
        }

        [HttpPost("insert-client-project")]
        public async Task<ActionResult<ApiResponse>> InsertClientProject([FromBody] InsertClientProjectRequest request)
        {
            var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 4000) { Direction = ParameterDirection.Output };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [ProjectManagement].[InsertClientProject] @ClientID, @ProjectID, @EmployeeID, @AssignedDate, @Success OUTPUT, @Message OUTPUT",
               new SqlParameter("@ClientID", (object)request.ClientID ?? DBNull.Value),

                new SqlParameter("@ProjectID", request.ProjectID),
                new SqlParameter("@EmployeeID", request.EmployeeID),
                new SqlParameter("@AssignedDate", (object)request.AssignedDate ?? DBNull.Value),
                successParam,
                messageParam
            );

            var success = (bool)successParam.Value;
            var message = messageParam.Value?.ToString();

            return Ok(new ApiResponse { Success = success, Message = message });
        }

        [HttpPut("update-client")]
        public async Task<ActionResult<ApiResponse>> UpdateClient([FromBody] UpdateClientRequest request)
        {
            var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 4000) { Direction = ParameterDirection.Output };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [ProjectManagement].[UpdateClient] @ClientID, @ClientName, @ContactEmail, @ContactPhone, @Success OUTPUT, @Message OUTPUT",
                new SqlParameter("@ClientID", request.ClientID),
                new SqlParameter("@ClientName", request.ClientName),
                new SqlParameter("@ContactEmail", (object)request.ContactEmail ?? DBNull.Value),
                new SqlParameter("@ContactPhone", (object)request.ContactPhone ?? DBNull.Value),
                successParam,
                messageParam
            );

            var success = (bool)successParam.Value;
            var message = messageParam.Value?.ToString();

            return Ok(new ApiResponse { Success = success, Message = message });
        }

        [HttpPut("update-client-project")]
        public async Task<ActionResult<ApiResponse>> UpdateClientProject([FromBody] UpdateClientProjectRequest request)
        {
            var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 4000) { Direction = ParameterDirection.Output };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [ProjectManagement].[UpdateClientProject] @ClientProjectID, @ClientID, @ProjectID, @EmployeeID, @AssignedDate, @Success OUTPUT, @Message OUTPUT",
                new SqlParameter("@ClientProjectID", request.ClientProjectID),
                new SqlParameter("@ClientID", request.ClientID),
                new SqlParameter("@ProjectID", request.ProjectID),
                new SqlParameter("@EmployeeID", request.EmployeeID),
                new SqlParameter("@AssignedDate", (object)request.AssignedDate ?? DBNull.Value),
                successParam,
                messageParam
            );

            var success = (bool)successParam.Value;
            var message = messageParam.Value?.ToString();

            return Ok(new ApiResponse { Success = success, Message = message });
        }

        [HttpDelete("delete-client/{clientId}")]
        public async Task<ActionResult<ApiResponse>> DeleteClient(int clientId)
        {
            var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 4000) { Direction = ParameterDirection.Output };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [ProjectManagement].[DeleteClient] @ClientID, @Success OUTPUT, @Message OUTPUT",
                new SqlParameter("@ClientID", clientId),
                successParam,
                messageParam
            );

            var success = (bool)successParam.Value;
            var message = messageParam.Value?.ToString();

            return Ok(new ApiResponse { Success = success, Message = message });
        }

        [HttpDelete("delete-client-project/{clientProjectId}")]
        public async Task<ActionResult<ApiResponse>> DeleteClientProject(int clientProjectId)
        {
            var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 4000) { Direction = ParameterDirection.Output };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [ProjectManagement].[DeleteClientProject] @ClientProjectID, @Success OUTPUT, @Message OUTPUT",
                new SqlParameter("@ClientProjectID", clientProjectId),
                successParam,
                messageParam
            );

            var success = (bool)successParam.Value;
            var message = messageParam.Value?.ToString();

            return Ok(new ApiResponse { Success = success, Message = message });
        }

        [HttpGet("client-projects-by-id/{clientId}")]
        public async Task<ActionResult<List<ClientProjectByIDDto>>> GetClientProjectsByID(int clientId)
        {
            var rawProjects = await _context.ClientProjectByIDDto
                .FromSqlRaw("EXEC [ProjectManagement].[GetClientProjectsByID] @ClientID",
                    new SqlParameter("@ClientID", clientId))
                .ToListAsync();

            // Deserialize the Employees JSON column into List<EmployeeDto>
            foreach (var project in rawProjects)
            {
                if (project.EmployeesJson != null)
                {
                    project.Employees = JsonSerializer.Deserialize<List<ClientEmployeeDto>>(project.EmployeesJson);
                }
            }

            return Ok(rawProjects);
        }



        // NEW: Get Client Issues
        [HttpGet("client-issues/{clientId}")]
        public async Task<ActionResult<List<ClientIssueDto>>> GetClientIssues(int clientId)
        {
            var clientIssues = await _context.ClientIssueDtos
                .FromSqlRaw("EXEC [ProjectManagement].[GetClientIssues] @ClientID",
                    new SqlParameter("@ClientID", clientId))
                .ToListAsync();

            return Ok(clientIssues);
        }

        [HttpGet("client-tasks/{clientId}")]
        public async Task<ActionResult<List<ClientTaskDto>>> GetClientTasks(int clientId)
        {
            var tasks = new List<ClientTaskDto>();

            using var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "[ProjectManagement].[GetClientTasksWithAssignments]";
            cmd.CommandType = CommandType.StoredProcedure;

            var param = cmd.CreateParameter();
            param.ParameterName = "@ClientID";
            param.Value = clientId;
            cmd.Parameters.Add(param);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var task = new ClientTaskDto
                {
                    ClientProjectID = reader.GetInt32(reader.GetOrdinal("ClientProjectID")),
                    ClientID = reader.GetInt32(reader.GetOrdinal("ClientID")),
                    ProjectID = reader.GetInt32(reader.GetOrdinal("ProjectID")),

                    // ✅ Project Name added
                    ProjectName = reader.IsDBNull(reader.GetOrdinal("ProjectName"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("ProjectName")),

                    ProjectAssignedDate = reader.GetDateTime(reader.GetOrdinal("ProjectAssignedDate")),
                    TaskID = reader.GetInt32(reader.GetOrdinal("TaskID")),
                    TaskName = reader.IsDBNull(reader.GetOrdinal("TaskName")) ? string.Empty : reader.GetString(reader.GetOrdinal("TaskName")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? string.Empty : reader.GetString(reader.GetOrdinal("Description")),
                    StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                    EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                    Priority = reader.IsDBNull(reader.GetOrdinal("Priority")) ? string.Empty : reader.GetString(reader.GetOrdinal("Priority")),
                    TaskStatus = reader.IsDBNull(reader.GetOrdinal("TaskStatus")) ? string.Empty : reader.GetString(reader.GetOrdinal("TaskStatus"))
                };

                // Deserialize assignments JSON
                if (!reader.IsDBNull(reader.GetOrdinal("Assignments")))
                {
                    var assignmentsJson = reader.GetString(reader.GetOrdinal("Assignments"));
                    task.Assignments = JsonSerializer.Deserialize<List<TaskAssignmentDto>>(assignmentsJson) ?? new List<TaskAssignmentDto>();
                }

                tasks.Add(task);
            }

            return Ok(tasks);
        }

    }

    // DTOs - Add these to your ProjectManagementApi.Dtos namespace
    public class ClientDto
    {
        public int ClientID { get; set; }
        public string? ClientName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class ClientProjectByIDDto
    {
        public int ClientProjectID { get; set; }
        public int ClientID { get; set; }
        public DateTime? AssignedDate { get; set; }
        public int ProjectID { get; set; }
        public string? ProjectName { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int StatusID { get; set; }
        public string? ProjectStatus { get; set; }

        public string? EmployeesJson { get; set; }

        // ✅ Add list of employees
        public List<ClientEmployeeDto>? Employees { get; set; }
    }

    public class ClientEmployeeDto
    {
        public int EmployeeID { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }



    public class ClientProjectDto
    {
        public int ClientProjectID { get; set; }
        public int ClientID { get; set; }         
        public string? ClientName { get; set; }
        public int? ProjectID { get; set; }
        public string? ProjectName { get; set; }
        public int? EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime? AssignedDate { get; set; }
    }

    public class ClientIssueDto
    {
        public int IssueID { get; set; }
        public int ClientProjectID { get; set; }
        public string IssueTitle { get; set; }
        public string IssueDescription { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? ResolvedByName { get; set; }
    } 
    public class InsertClientRequest
    {
        public string ClientName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class UpdateClientRequest
    {
        public int ClientID { get; set; }
        public string ClientName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class InsertClientProjectRequest
    {
        public int? ClientID { get; set; }
        public int ProjectID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime? AssignedDate { get; set; }
    }

    public class UpdateClientProjectRequest
    {
        public int ClientProjectID { get; set; }
        public int ClientID { get; set; }
        public int ProjectID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime? AssignedDate { get; set; }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class DashboardProjectDto
    {
        public int? ClientProjectID { get; set; }
        public int? ProjectID { get; set; }
        public string? ProjectName { get; set; } = string.Empty;
        public string? ProjectDescription { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? StatusID { get; set; }
        public string? ProjectStatus { get; set; } = string.Empty;
    }

    public class DashboardProjectCountsDto
    {
        public int? TotalProjects { get; set; }
        public int? PlannedProjects { get; set; }
        public int? InProgressProjects { get; set; }
        public int? CompletedProjects { get; set; }
        public int? OnHoldProjects { get; set; }
    }

    public class DashboardIssueDto
    {
        public int? IssueID { get; set; }
        public int? ClientProjectID { get; set; }
        public string? IssueTitle { get; set; } = string.Empty;
        public string? IssueDescription { get; set; } = string.Empty;
        public string? Status { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? ResolvedByName { get; set; }
    }

    public class DashboardIssueCountsDto
    {
        public int? TotalIssues { get; set; }
        public int? OpenIssues { get; set; }
        public int? ResolvedIssues { get; set; }
    }

    public class ClientDashboardDto
    {
        public List<DashboardProjectDto> Projects { get; set; } = new();
        public DashboardProjectCountsDto ProjectCounts { get; set; } = new();
        public List<DashboardIssueDto> Issues { get; set; } = new();
        public DashboardIssueCountsDto IssueCounts { get; set; } = new();
    }

    public class ClientTaskDto
    {
        public int ClientProjectID { get; set; }
        public int ClientID { get; set; }
        public int ProjectID { get; set; }

        public string ProjectName { get; set; } = string.Empty;


        public DateTime ProjectAssignedDate { get; set; }


        public int TaskID { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string TaskStatus { get; set; } = string.Empty;

        // Nested assignments
        public List<TaskAssignmentDto> Assignments { get; set; } = new();
    }

    public class TaskAssignmentDto
    {
        public int AssignmentID { get; set; }
        public int AssignedEmployeeID { get; set; }
        public string AssignedEmployeeName { get; set; } = string.Empty;
        public string AssignedEmployeeEmail { get; set; } = string.Empty;
        public DateTime TaskAssignedDate { get; set; }
        public DateTime? DueDate { get; set; }
    }



}