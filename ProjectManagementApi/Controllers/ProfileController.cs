using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.DataContext;
using System.Data;

namespace ProjectManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly PMDataContext _context;

        public ProfileController(PMDataContext context)
        {
            _context = context;
        }

        [HttpGet("GetEmployeeProfile/{employeeId}")]
        public async Task<IActionResult> GetEmployeeProfile(int employeeId)
        {
            var profile = new EmployeeProfileDto
            {
                Projects = new List<ProjectClientDto>(),
                Tasks = new List<TaskDto>(),
                Issues = new List<IssueDto>()
            };

            using (var connection = _context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "[ProjectManagement].[GetEmployeeProfileOverview]";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@EmployeeID", employeeId));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // 1️⃣ EMPLOYEE INFO
                        if (await reader.ReadAsync())
                        {
                            profile.EmployeeInfo = new EmployeeInfoDto
                            {
                                FullName = reader["Full Name"]?.ToString() ?? "N/A",
                                EmailAddress = reader["Email Address"]?.ToString() ?? "N/A",
                                Role = reader["Role"]?.ToString() ?? "N/A",
                                ProfilePic = reader.IsDBNull(reader.GetOrdinal("Profile Picture"))
         ? null
         : (byte[])reader["Profile Picture"]
                            };

                        }

                        // 2️⃣ PROJECTS
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                profile.Projects.Add(new ProjectClientDto
                                {
                                    ProjectName = reader["Project Name"]?.ToString(),
                                    ProjectDescription = reader["Project Description"]?.ToString(),
                                    ProjectStatus = reader["Project Status"]?.ToString(),
                                    ClientName = reader["Client Name"]?.ToString(),
                                    ClientEmail = reader["Client Email"]?.ToString(),
                                    ClientPhone = reader["Client Phone"]?.ToString()
                                });
                            }
                        }

                        // 3️⃣ TASKS
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                profile.Tasks.Add(new TaskDto
                                {
                                    TaskName = reader["Task Name"]?.ToString(),
                                    TaskDescription = reader["Task Description"]?.ToString(),
                                    TaskStatus = reader["Task Status"]?.ToString(),
                                    Priority = reader["Priority"]?.ToString(),
                                    ProjectName = reader["Project Name"]?.ToString()
                                });
                            }
                        }

                        // 4️⃣ ISSUES
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                profile.Issues.Add(new IssueDto
                                {
                                    IssueTitle = reader["Issue Title"]?.ToString(),
                                    IssueDescription = reader["Issue Description"]?.ToString(),
                                    CurrentStatus = reader["Current Status"]?.ToString(),
                                    ProjectName = reader["Project Name"]?.ToString(),
                                    ClientName = reader["Client Name"]?.ToString(),
                                    ResolutionNotes = reader["Resolution Notes"]?.ToString()
                                });
                            }
                        }
                    }
                }
            }

            if (profile.EmployeeInfo == null)
                return NotFound(new { Message = $"Employee with ID {employeeId} not found." });

            return Ok(profile);
        }

        [HttpPut("employee-profilepic/{employeeId}")]
        public async Task<IActionResult> UpdateEmployeeProfilePic(int employeeId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            using (var connection = (SqlConnection)_context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("[ProjectManagement].[UpdateEmployeeProfilePic]", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@EmployeeID", SqlDbType.Int)
                    {
                        Value = employeeId
                    });

                    command.Parameters.Add(new SqlParameter("@ProfilePic", SqlDbType.VarBinary, -1)
                    {
                        Value = imageBytes
                    });

                    var successParam = new SqlParameter("@Success", SqlDbType.Bit)
                    {
                        Direction = ParameterDirection.Output
                    };

                    var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 4000)
                    {
                        Direction = ParameterDirection.Output
                    };

                    command.Parameters.Add(successParam);
                    command.Parameters.Add(messageParam);

                    await command.ExecuteNonQueryAsync();

                    return Ok(new
                    {
                        Success = (bool)successParam.Value,
                        Message = messageParam.Value?.ToString()
                    });
                }
            }
        }


        [HttpGet("profile/{clientId}")]
        public async Task<IActionResult> GetClientProfile(int clientId)
        {
            ClientsprofileDto clientProfile = null;

            using (var conn = _context.Database.GetDbConnection())
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "[ProjectManagement].[GetClientProfile]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ClientID", clientId));

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            clientProfile = new ClientsprofileDto
                            {
                                ClientID = reader.GetInt32(reader.GetOrdinal("ClientID")),
                                ClientName = reader["ClientName"] as string,
                                ContactEmail = reader["ContactEmail"] as string,
                                ContactPhone = reader["ContactPhone"] as string,
                                ProfilePic = reader.IsDBNull(reader.GetOrdinal("ProfilePic"))
                                    ? null
                                    : (byte[])reader["ProfilePic"]
                            };
                        }
                    }
                }
            }

            if (clientProfile == null)
                return NotFound(new { Message = $"Client with ID {clientId} not found." });

            return Ok(clientProfile);
        }


        [HttpPut("client-profilepic/{clientId}")]
        public async Task<IActionResult> UpdateClientProfilePic(int clientId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            using (var connection = (SqlConnection)_context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("[ProjectManagement].[UpdateClientProfilePic]", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@ClientID", SqlDbType.Int) { Value = clientId });
                    command.Parameters.Add(new SqlParameter("@ProfilePic", SqlDbType.VarBinary, -1) { Value = imageBytes });

                    var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                    var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 4000) { Direction = ParameterDirection.Output };

                    command.Parameters.Add(successParam);
                    command.Parameters.Add(messageParam);

                    await command.ExecuteNonQueryAsync();

                    return Ok(new
                    {
                        Success = (bool)successParam.Value,
                        Message = messageParam.Value?.ToString()
                    });
                }
            }
        }

        // -------------------------------
        // PUT: Update Company Profile Picture
        // -------------------------------
        [HttpPut("company-profilepic")]
        public async Task<IActionResult> UpdateCompanyProfilePic(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            using (var connection = (SqlConnection)_context.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("[ProjectManagement].[UpdateCompanyProfilePic]", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@ProfilePic", SqlDbType.VarBinary, -1) { Value = imageBytes });

                    var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                    var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 4000) { Direction = ParameterDirection.Output };

                    command.Parameters.Add(successParam);
                    command.Parameters.Add(messageParam);

                    await command.ExecuteNonQueryAsync();

                    return Ok(new
                    {
                        Success = (bool)successParam.Value,
                        Message = messageParam.Value?.ToString()
                    });
                }
            }
        }



    }

    // ==============================================
    // ✅ DTOs
    // ==============================================

    public class EmployeeProfileDto
    {
        public EmployeeInfoDto EmployeeInfo { get; set; }
        public List<ProjectClientDto> Projects { get; set; }
        public List<TaskDto> Tasks { get; set; }
        public List<IssueDto> Issues { get; set; }
    }
    public class EmployeeInfoDto
    {
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string Role { get; set; }

        public byte[]? ProfilePic { get; set; } // ✅ added
    }


    public class ProjectClientDto
    {
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public string ProjectStatus { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientPhone { get; set; }
    }

    public class TaskDto
    {
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public string TaskStatus { get; set; }
        public string Priority { get; set; }
        public string ProjectName { get; set; }
    }

    public class IssueDto
    {
        public string IssueTitle { get; set; }
        public string IssueDescription { get; set; }
        public string CurrentStatus { get; set; }
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public string ResolutionNotes { get; set; }
    }

    // DTOs
    // ==============================================
    public class ProfilePicRequest
    {
        public string? ProfilePic { get; set; }
    }

    public class ClientsprofileDto
    {
        public int ClientID { get; set; }
        public string? ClientName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }

        public byte[]? ProfilePic { get; set; } // ✅ binary
    }

}