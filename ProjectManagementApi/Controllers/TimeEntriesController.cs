using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.DataContext;
using ProjectManagementApi.DTOs;

namespace ProjectManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeEntryController : ControllerBase
    {
        private readonly PMDataContext _context;

        public TimeEntryController(PMDataContext context)
        {
            _context = context;
        }

        // 📌 GET all time entries
        [HttpGet("GetAllTimeEntries")]
        public async Task<IActionResult> GetAllTimeEntries()
        {
            var entries = await _context.TimeEntryResults
                .FromSqlRaw("EXEC ProjectManagement.GetAllTimeEntries")
                .ToListAsync();
            return Ok(entries);
        }
        [HttpGet("GetCompletedTimeEntries")]
        public async Task<IActionResult> GetCompletedTimeEntries()
        {
            var entries = await _context.TimeEntryResults
                .FromSqlRaw("EXEC ProjectManagement.GetCompletedTimeEntries")
                .ToListAsync();
            return Ok(entries);
        }

        // 📌 GET time entries by project
        [HttpGet("GetTimeEntriesByProject/{projectId}")]
        public async Task<IActionResult> GetTimeEntriesByProject(int projectId)
        {
            var entries = await _context.TimeEntryResults
                .FromSqlRaw("EXEC ProjectManagement.GetTimeEntriesByProject @ProjectID={0}", projectId)
                .ToListAsync();
            return Ok(entries);
        }

        // 📌 ADD time entry
        [HttpPost("AddTimeEntry")]
        public async Task<IActionResult> AddTimeEntry([FromBody] AddTimeEntryDto dto)
        {
            var result = await _context.DbResponse
                .FromSqlRaw(
                    "EXEC ProjectManagement.AddTimeEntry @TaskID={0}, @EmployeeID={1}, @DateWorked={2}, @HoursWorked={3}, @Notes={4}",
                    dto.TaskID, dto.EmployeeID, dto.DateWorked, dto.HoursWorked, dto.Notes
                ).ToListAsync();

            return Ok(result);
        }

        // 📌 UPDATE time entry
        [HttpPut("UpdateTimeEntry")]
        public async Task<IActionResult> UpdateTimeEntry([FromBody] UpdateTimeEntryDto dto)
        {
            var result = await _context.DbResponse
                .FromSqlRaw(
                    "EXEC ProjectManagement.UpdateTimeEntry @TimeEntryID={0}, @DateWorked={1}, @HoursWorked={2}, @Notes={3}",
                    dto.TimeEntryID, dto.DateWorked, dto.HoursWorked, dto.Notes
                ).ToListAsync();

            return Ok(result);
        }

        // 📌 DELETE time entry
        [HttpDelete("DeleteTimeEntry/{timeEntryId}")]
        public async Task<IActionResult> DeleteTimeEntry(int timeEntryId, [FromQuery] string userId = "SYSTEM")
        {
            var result = await _context.DbResponse
                .FromSqlRaw(
                    "EXEC ProjectManagement.DeleteTimeEntry @TimeEntryID={0}, @UserID={1}",
                    timeEntryId, userId
                ).ToListAsync();

            return Ok(result);
        }

        // 📌 MARK task as completed (Admin action)
        [HttpPost("MarkTaskCompleted")]
        public async Task<IActionResult> MarkTaskCompleted([FromBody] MarkTaskCompletedDto dto)
        {
            if (dto.TaskID <= 0)
                return BadRequest("Invalid TaskID.");

            var result = await _context.DbResponse
                .FromSqlRaw(
                    "EXEC ProjectManagement.MarkTaskCompleted @TaskID={0}",
                    dto.TaskID
                )
                .ToListAsync();

            return Ok(result);
        }

    }
}


namespace ProjectManagementApi.DTOs
{
    public class AddTimeEntryDto
    {
        public int TaskID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime DateWorked { get; set; }
        public decimal HoursWorked { get; set; }
        public string? Notes { get; set; }
    }

    public class MarkTaskCompletedDto
    {
        public int TaskID { get; set; }
    }

    public class UpdateTimeEntryDto
    {
        public int TimeEntryID { get; set; }
        public DateTime DateWorked { get; set; }
        public decimal HoursWorked { get; set; }
        public string? Notes { get; set; }
    }

    public class TimeEntryResultDto
    {
        public int TimeEntryID { get; set; }
        public int TaskID { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime DateWorked { get; set; }
        public decimal HoursWorked { get; set; }
        public string? Notes { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
    }

    public class DBResponse
    {
        public bool Success { get; set; }
        public string MessageType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Data { get; set; }
    }
}
