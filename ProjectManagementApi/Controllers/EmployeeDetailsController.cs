using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;
using ProjectManagementApi.DataContext;

namespace ProjectManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeDetailsController : ControllerBase
    {
        private readonly PMDataContext _context;

        public EmployeeDetailsController(PMDataContext context)
        {
            _context = context;
        }

        // ====================================
        // DASHBOARD ENDPOINTS - FIXED
        // ====================================
        [HttpGet("dashboard-summary/{employeeId}")]
        public async Task<ActionResult<EmployeeDashboardSummaryResponse>> GetDashboardSummary(int employeeId)
        {
            var response = new EmployeeDashboardSummaryResponse();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "[ProjectManagement].[usp_GetEmployeeDashboardSummary]";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@EmployeeID", employeeId));

                if (command.Connection.State != ConnectionState.Open)
                    await command.Connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    // RESULT SET 1: Summary Statistics
                    if (await reader.ReadAsync())
                    {
                        response.SummaryStatistics = new EmployeeSummaryDto
                        {
                            EmployeeID = (int)reader["EmployeeID"],
                            FullName = reader["FullName"] == DBNull.Value ? null : reader["FullName"].ToString(),
                            Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                            Role = reader["Role"] == DBNull.Value ? null : reader["Role"].ToString(),

                            // PROJECT COUNTS
                            TotalProjects = (int)reader["TotalProjects"],
                            ActiveProjects = (int)reader["ActiveProjects"],
                            CompletedProjects = (int)reader["CompletedProjects"],

                            // TASK COUNTS
                            TotalTasks = (int)reader["TotalTasks"],
                            ActiveTasks = (int)reader["ActiveTasks"],
                            CompletedTasks = (int)reader["CompletedTasks"],
                            OverdueTasks = (int)reader["OverdueTasks"],

                            // ISSUE COUNTS
                            TotalIssues = (int)reader["TotalIssues"],
                            OpenIssues = (int)reader["OpenIssues"],
                            InProgressIssues = (int)reader["InProgressIssues"],
                            IssuesResolved = (int)reader["IssuesResolved"],
                            

                            // TIME TRACKING
                            TotalHoursWorked = (decimal)reader["TotalHoursWorked"],
                            HoursWorkedLastMonth = (decimal)reader["HoursWorkedLastMonth"],
                            HoursWorkedLastWeek = (decimal)reader["HoursWorkedLastWeek"],
                            HoursWorkedToday = (decimal)reader["HoursWorkedToday"],

                            // CLIENT & ALLOCATION
                            TotalClientsServed = (int)reader["TotalClientsServed"],
                            CurrentAllocationPercentage = (decimal)reader["CurrentAllocationPercentage"]
                        };
                    }

                    // RESULT SET 2: Recent Activity
                    await reader.NextResultAsync();
                    while (await reader.ReadAsync())
                    {
                        response.RecentActivities.Add(new RecentActivityDto
                        {
                            ActivityType = reader["ActivityType"].ToString(),
                            ActivityDescription = reader["ActivityDescription"].ToString(),
                            ActivityDate = (DateTime)reader["ActivityDate"],
                            RelatedEntityID = (int)reader["RelatedEntityID"],
                            RelatedEntityName = reader["RelatedEntityName"].ToString()
                        });
                    }

                    // RESULT SET 3: Hours Worked Chart Data
                    await reader.NextResultAsync();
                    while (await reader.ReadAsync())
                    {
                        response.HoursWorkedChartData.Add(new HoursWorkedChartDto
                        {
                            WorkDate = (DateTime)reader["WorkDate"],
                            TotalHours = (decimal)reader["TotalHours"],
                            TasksWorkedOn = (int)reader["TasksWorkedOn"]
                        });
                    }

                    // RESULT SET 4: Task Status Distribution
                    await reader.NextResultAsync();
                    while (await reader.ReadAsync())
                    {
                        response.TaskStatusDistribution.Add(new StatusDistributionDto
                        {
                            StatusName = reader["StatusName"].ToString(),
                            TaskCount = (int)reader["TaskCount"]
                        });
                    }

                    // RESULT SET 5: Project Status Distribution
                    await reader.NextResultAsync();
                    while (await reader.ReadAsync())
                    {
                        response.ProjectStatusDistribution.Add(new ProjectStatusDistributionDto
                        {
                            StatusDescription = reader["StatusDescription"].ToString(),
                            ProjectCount = (int)reader["ProjectCount"]
                        });
                    }
                }
            }

            return Ok(response);
        }
        // ====================================
        // MY PROJECTS ENDPOINTS
        // ====================================

        [HttpGet("my-projects/{employeeId}")]
        public async Task<ActionResult<List<MyProjectDto>>> GetMyProjects(int employeeId)
        {
            var projects = await _context.MyProjectDtos
                .FromSqlRaw("EXEC [ProjectManagement].[usp_GetMyProjects] @EmployeeID",
                    new SqlParameter("@EmployeeID", employeeId))
                .ToListAsync();

            return Ok(projects);
        }

        // ====================================
        // MY TASKS ENDPOINTS
        // ====================================

        [HttpGet("my-tasks/{employeeId}")]
        public async Task<ActionResult<List<MyTaskDto>>> GetMyTasks(int employeeId)
        {
            var tasks = await _context.MyTaskDtos
                .FromSqlRaw("EXEC [ProjectManagement].[usp_GetMyTasks] @EmployeeID",
                    new SqlParameter("@EmployeeID", employeeId))
                .ToListAsync();

            return Ok(tasks);
        }

        // ====================================
        // MY ISSUES ENDPOINTS
        // ====================================

        [HttpGet("my-issues/{employeeId}")]
        public async Task<ActionResult<List<MyIssueDto>>> GetMyIssues(int employeeId)
        {
            var issues = await _context.MyIssueDtos
                .FromSqlRaw("EXEC [ProjectManagement].[usp_GetMyIssues] @EmployeeID",
                    new SqlParameter("@EmployeeID", employeeId))
                .ToListAsync();

            return Ok(issues);
        }

        // ====================================
        // TIME ENTRIES ENDPOINTS
        // ====================================

        [HttpGet("task-time-entries/{employeeId}")]
        public async Task<ActionResult<List<TaskTimeEntryDto>>> GetTaskTimeEntries(int employeeId)
        {
            var timeEntries = await _context.TaskTimeEntryDtos
                .FromSqlRaw("EXEC [ProjectManagement].[usp_GetTaskTimeEntries] @EmployeeID",
                    new SqlParameter("@EmployeeID", employeeId))
                .ToListAsync();

            return Ok(timeEntries);
        }

        // ====================================
        // FILTER OPTIONS ENDPOINTS
        // ====================================

        [HttpGet("filter-options/{employeeId}")]
        public async Task<ActionResult<EmployeeFilterOptionsResponse>> GetFilterOptions(int employeeId)
        {
            var response = new EmployeeFilterOptionsResponse();

            // Result Set 1: Projects
            response.Projects = await _context.ProjectFilterDtos
                .FromSqlRaw("EXEC [ProjectManagement].[usp_GetEmployeeFilterOptions] @EmployeeID",
                    new SqlParameter("@EmployeeID", employeeId))
                .ToListAsync();

            // Result Set 2: Clients
            response.Clients = await _context.ClientFilterDtos
                .FromSqlRaw("EXEC [ProjectManagement].[usp_GetEmployeeFilterOptions] @EmployeeID",
                    new SqlParameter("@EmployeeID", employeeId))
                .ToListAsync();

            // Result Set 3: Task Statuses
            response.TaskStatuses = await _context.TaskStatusFilterDtos
                .FromSqlRaw("EXEC [ProjectManagement].[usp_GetEmployeeFilterOptions] @EmployeeID",
                    new SqlParameter("@EmployeeID", employeeId))
                .ToListAsync();

            // Result Set 4: Project Statuses
            response.ProjectStatuses = await _context.ProjectStatusFilterDtos
                .FromSqlRaw("EXEC [ProjectManagement].[usp_GetEmployeeFilterOptions] @EmployeeID",
                    new SqlParameter("@EmployeeID", employeeId))
                .ToListAsync();

            // Result Set 5: Billing Statuses
            response.BillingStatuses = await _context.BillingStatusFilterDtos
                .FromSqlRaw("EXEC [ProjectManagement].[usp_GetEmployeeFilterOptions] @EmployeeID",
                    new SqlParameter("@EmployeeID", employeeId))
                .ToListAsync();

            // Result Set 6: Priorities
            response.Priorities = await _context.PriorityFilterDtos
                .FromSqlRaw("EXEC [ProjectManagement].[usp_GetEmployeeFilterOptions] @EmployeeID",
                    new SqlParameter("@EmployeeID", employeeId))
                .ToListAsync();

            return Ok(response);
        }
    }

    // ====================================
    // DTOs - DASHBOARD
    // ====================================
    // ====================================
    // DTO CLASSES (Make sure these match!)
    // ====================================
    public class EmployeeSummaryDto
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        // Project counts
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }

        // Task counts
        public int TotalTasks { get; set; }
        public int ActiveTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }

        // Issue counts
        public int TotalIssues { get; set; }
        public int OpenIssues { get; set; }
        public int InProgressIssues { get; set; }
        public int IssuesResolved { get; set; }
 

        // Time tracking
        public decimal TotalHoursWorked { get; set; }
        public decimal HoursWorkedLastMonth { get; set; }
        public decimal HoursWorkedLastWeek { get; set; }
        public decimal HoursWorkedToday { get; set; }

        // Client & allocation
        public int TotalClientsServed { get; set; }
        public decimal CurrentAllocationPercentage { get; set; }
    }

    public class EmployeeDashboardSummaryResponse
    {
        public EmployeeSummaryDto SummaryStatistics { get; set; }
        public List<RecentActivityDto> RecentActivities { get; set; } = new List<RecentActivityDto>();
        public List<HoursWorkedChartDto> HoursWorkedChartData { get; set; } = new List<HoursWorkedChartDto>();
        public List<StatusDistributionDto> TaskStatusDistribution { get; set; } = new List<StatusDistributionDto>();
        public List<ProjectStatusDistributionDto> ProjectStatusDistribution { get; set; } = new List<ProjectStatusDistributionDto>();
    }

    public class RecentActivityDto
    {
        public string ActivityType { get; set; }
        public string ActivityDescription { get; set; }
        public DateTime ActivityDate { get; set; }
        public int RelatedEntityID { get; set; }
        public string RelatedEntityName { get; set; }
    }

    public class HoursWorkedChartDto
    {
        public DateTime WorkDate { get; set; }
        public decimal TotalHours { get; set; }
        public int TasksWorkedOn { get; set; }
    }

    public class StatusDistributionDto
    {
        public string StatusName { get; set; }
        public int TaskCount { get; set; }
    }

    public class ProjectStatusDistributionDto
    {
        public string StatusDescription { get; set; }
        public int ProjectCount { get; set; }
    }

    // ====================================
    // DTOs - MY PROJECTS
    // ====================================

    public class MyProjectDto
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string? ProjectDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? ProjectStatus { get; set; }
        public int? StatusID { get; set; }

        public int ClientID { get; set; }
        public string ClientName { get; set; }
        public string? ClientEmail { get; set; }
        public string? ClientPhone { get; set; }

        public DateTime AssignedDate { get; set; }

        // Task stats (employee only)
        public int MyTotalTasks { get; set; }
        public int MyCompletedTasks { get; set; }
        public int MyActiveTasks { get; set; }

        // Time tracking
        public decimal TotalHoursWorked { get; set; }
        public DateTime? LastWorkedDate { get; set; }

 

        // Issues & progress
        public int OpenIssues { get; set; }
        public decimal MyProgressPercent { get; set; }

        // Dates
        public int DaysSinceStart { get; set; }
        public int? DaysUntilEnd { get; set; }

        // Team info
        public int TeamSize { get; set; }
        public string? TeamMembers { get; set; }
    }


    // ====================================
    // DTOs - MY TASKS
    // ====================================

    public class MyTaskDto
    {
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Priority { get; set; }
        public string? TaskStatus { get; set; }
        public int? TaskStatusID { get; set; }
        public DateTime AssignedDate { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string? ProjectStatus { get; set; }
        public int? ClientID { get; set; }
        public string? ClientName { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public int TimeEntryCount { get; set; }
        public DateTime? LastWorkedDate { get; set; }
        public decimal? LastTimeEntryHours { get; set; }
        public string? LastTimeEntryNotes { get; set; }
        public decimal HoursWorkedLast7Days { get; set; }
        public int DaysSinceStart { get; set; }
        public int? DaysUntilDue { get; set; }
        public int IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
        public int TeamMembersAssigned { get; set; }
        public string? OtherAssignedEmployees { get; set; }
        public decimal TimeProgressPercent { get; set; }
    }

    // ====================================
    // DTOs - MY ISSUES
    // ====================================

    public class MyIssueDto
    {
        public int IssueID { get; set; }
        public string IssueTitle { get; set; }
        public string IssueDescription { get; set; }
        public string IssueStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public int ClientID { get; set; }
        public string ClientName { get; set; }
        public string? ClientEmail { get; set; }
        public string? ClientPhone { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string? ProjectStatus { get; set; }
        public int IsResolvedByMe { get; set; }
        public int? ResolvedByEmployeeID { get; set; }
        public string? ResolvedByEmployeeName { get; set; }
        public string? ResolvedByEmployeeEmail { get; set; }
        public int? MyResolutionID { get; set; }
        public string? MyResolutionNotes { get; set; }
        public DateTime? MyResolutionDate { get; set; }
        public int TotalResolutionAttempts { get; set; }
        public int DaysSinceCreated { get; set; }
        public int DaysToResolution { get; set; }
        public string CalculatedPriority { get; set; }
        public int ClientProjectID { get; set; }
        public DateTime ProjectAssignedDate { get; set; }
        public int IsActive { get; set; }
    }

    // ====================================
    // DTOs - TIME ENTRIES
    // ====================================

    public class TaskTimeEntryDto
    {
        public int TimeEntryID { get; set; }
        public DateTime DateWorked { get; set; }
        public decimal HoursWorked { get; set; }
        public string? Notes { get; set; }
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public string? Priority { get; set; }
        public string? TaskStatus { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int? ClientID { get; set; }
        public string? ClientName { get; set; }
        public decimal RunningTotalHours { get; set; }
    }

    // ====================================
    // DTOs - FILTER OPTIONS
    // ====================================

    public class EmployeeFilterOptionsResponse
    {
        public List<ProjectFilterDto> Projects { get; set; } = new();
        public List<ClientFilterDto> Clients { get; set; } = new();
        public List<TaskStatusFilterDto> TaskStatuses { get; set; } = new();
        public List<ProjectStatusFilterDto> ProjectStatuses { get; set; } = new();
        public List<BillingStatusFilterDto> BillingStatuses { get; set; } = new();
        public List<PriorityFilterDto> Priorities { get; set; } = new();
    }

    public class ProjectFilterDto
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string? ProjectStatus { get; set; }
    }

    public class ClientFilterDto
    {
        public int ClientID { get; set; }
        public string ClientName { get; set; }
        public string? ContactEmail { get; set; }
    }

    public class TaskStatusFilterDto
    {
        public int TaskStatusID { get; set; }
        public string StatusName { get; set; }
    }

    public class ProjectStatusFilterDto
    {
        public int StatusID { get; set; }
        public string Description { get; set; }
    }

    public class BillingStatusFilterDto
    {
        public int BillingStatusID { get; set; }
        public string StatusName { get; set; }
    }

    public class PriorityFilterDto
    {
        public string Priority { get; set; }
    }
}