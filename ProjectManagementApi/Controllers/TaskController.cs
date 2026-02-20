using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.DataContext;
using ProjectManagementApi.DTOs;
 

namespace ProjectManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly PMDataContext _context;

        public TaskController(PMDataContext context)
        {
            _context = context;
        }

        // 📌 GET all tasks
        [HttpGet("GetAllTasks")]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _context.GetAllTasks.FromSqlRaw("EXEC ProjectManagement.GetAllTasks").ToListAsync();
            return Ok(tasks);
        }

        // 📌 GET all task status
        [HttpGet("GetAllTaskStatus")]
        public async Task<IActionResult> GetAllTaskStatus()
        {
            var statuses = await _context.GetAllTaskStatus.FromSqlRaw("EXEC ProjectManagement.GetAllTaskStatus").ToListAsync();
            return Ok(statuses);
        }

        // 📌 ADD Task
        [HttpPost("AddTask")]
        public async Task<IActionResult> AddTask([FromBody] AddTaskDto dto)
        {
            var result = await _context.DbResponse.FromSqlRaw(
                "EXEC ProjectManagement.AddTask @ProjectID={0}, @TaskName={1}, @Description={2}, @StartDate={3}, @EndDate={4}, @Priority={5}, @TaskStatusID={6}, @UserID={7}",
                dto.ProjectID, dto.TaskName, dto.Description, dto.StartDate, dto.EndDate, dto.Priority, dto.TaskStatusID, dto.UserID
            ).ToListAsync();

            return Ok(result);
        }
        // ProjectManagementApi/Controllers/TaskController.cs

        // ...

        // 📌 UPDATE Task
        [HttpPut("UpdateTask")]
        public async Task<IActionResult> UpdateTask([FromBody] UpdateTaskDto dto)
        {
            // *** CHANGE the DbSet from RowsAffected to TaskUpdateResponses ***
            var result = await _context.TaskUpdateResponses.FromSqlRaw(
                "EXEC ProjectManagement.UpdateTask @TaskID={0}, @TaskName={1}, @Description={2}, @StartDate={3}, @EndDate={4}, @Priority={5}, @TaskStatusID={6}, @UserID={7}",
                dto.TaskID, dto.TaskName, dto.Description, dto.StartDate, dto.EndDate, dto.Priority, dto.TaskStatusID, dto.UserID
            ).ToListAsync();

            // Since the result is a list with one item, return that item.
            var response = result.FirstOrDefault();

            if (response == null)
            {
                return StatusCode(500, new { Message = "Database did not return a response.", SuccessCode = 0 });
            }

            if (response.SuccessCode == 1)
            {
                return Ok(response);
            }
            else // SuccessCode is 0 or other error code
            {
                // For a non-success response, it's often better to return a 400 Bad Request
                return BadRequest(response);
            }
        }

        // 📌 DELETE Task
        [HttpDelete("DeleteTask/{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId, [FromQuery] string userId = "SYSTEM")
        {
            var result = await _context.DbResponse.FromSqlRaw(
                "EXEC ProjectManagement.DeleteTask @TaskID={0}, @UserID={1}", taskId, userId
            ).ToListAsync();

            return Ok(result);
        }
 
     [HttpGet("GetAllTaskAssignments")]
     public async Task<IActionResult> GetAllTaskAssignments()
        {
            var assignments = await _context.GetAllTaskAssignments.FromSqlRaw("EXEC ProjectManagement.GetAllTaskAssignments").ToListAsync();
            return Ok(assignments);
        }

        // 📌 ADD Task Assignment
        [HttpPost("AddTaskAssignment")]
        public async Task<IActionResult> AddTaskAssignment([FromBody] AddTaskAssignmentDto dto)
        {
            var result = await _context.DbResponse.FromSqlRaw(
                "EXEC ProjectManagement.AddTaskAssignment @TaskID={0}, @EmployeeID={1}, @AssignedDate={2}, @ForceAssign={3}",
                dto.TaskID, dto.EmployeeID, dto.AssignedDate ?? DateTime.Now, dto.ForceAssign
            ).ToListAsync();

            return Ok(result);
        }


        [HttpPut("UpdateTaskAssignment")]
        public async Task<IActionResult> UpdateTaskAssignment([FromBody] UpdateTaskAssignmentDto dto)
        {
            // 1. Change to use the TaskUpdateResponse DbSet
            var result = await _context.TaskUpdateResponses.FromSqlRaw(
                "EXEC ProjectManagement.UpdateTaskAssignment @AssignmentID={0}, @TaskID={1}, @EmployeeID={2}, @AssignedDate={3}",
                dto.AssignmentID, dto.TaskID, dto.EmployeeID, dto.AssignedDate
            ).ToListAsync();

            // The result should contain a list with a single TaskUpdateResponse object.
            var response = result.FirstOrDefault();

            if (response == null)
            {
                // Handle case where no result was returned (e.g., database connection issue)
                return StatusCode(500, new { Message = "Database did not return a response.", SuccessCode = 0 });
            }

            // 2. Return a structured result based on the SuccessCode
            if (response.SuccessCode == 1)
            {
                // Success
                return Ok(response);
            }
            else
            {
                // Failure or warning (e.g., AssignmentID not found)
                return BadRequest(response);
            }
        }


        // 📌 DELETE Task Assignment
        [HttpDelete("DeleteTaskAssignment/{assignmentId}")]
        public async Task<IActionResult> DeleteTaskAssignment(int assignmentId, [FromQuery] string userId = "SYSTEM")
        {
            var result = await _context.DbResponse.FromSqlRaw(
                "EXEC ProjectManagement.DeleteTaskAssignment @AssignmentID={0}, @UserID={1}",
                assignmentId, userId
            ).ToListAsync();

            return Ok(result);
        }
    }
}
namespace ProjectManagementApi.DTOs
{
    public class AddTaskDto
    {
        public int ProjectID { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Priority { get; set; }
        public int? TaskStatusID { get; set; }
        public string UserID { get; set; } = "SYSTEM";
    }

    public class UpdateTaskDto : AddTaskDto
    {
        public int TaskID { get; set; }
    }
    public class AddTaskAssignmentDto
    {
        public int TaskID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime? AssignedDate { get; set; }
 
        public bool ForceAssign { get; set; } = false;
    }

    public class UpdateTaskAssignmentDto : AddTaskAssignmentDto
    {
        public int AssignmentID { get; set; }
    }

}

public class GetAllTasksResult
{
    public int TaskID { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Priority { get; set; }
    public int ProjectID { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int? TaskStatusID { get; set; }
    public string? TaskStatus { get; set; }
    public string? AssignedEmployees { get; set; }
    public decimal TotalHoursWorked { get; set; }
}

public class GetAllTaskAssignmentsResult
{
    public int AssignmentID { get; set; }
    public int TaskID { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public int ProjectID { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int EmployeeID { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
}

public class GetAllTaskStatusResult
{
    public int TaskStatusID { get; set; }
    public string StatusName { get; set; } = string.Empty;
}

public class DbResponse
{
    public bool Success { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Data { get; set; }
}

public class TaskUpdateResponse
{
    // Matches the 'Message' column (NVARCHAR)
    public string Message { get; set; } = string.Empty;

    // Matches the 'SuccessCode' column (INT)
    public int SuccessCode { get; set; }
}