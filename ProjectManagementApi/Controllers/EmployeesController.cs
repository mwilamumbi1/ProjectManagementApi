using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.DataContext;
using ProjectManagementApi.dTOs;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly PMDataContext _context;

        public EmployeesController(PMDataContext context)
        {
            _context = context;
        }

        // 📌 Get all employees
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var result = await _context.EmployeesDtos
                .FromSqlRaw("EXEC [ProjectManagement].[GetAllEmployees]")
                .ToListAsync();

            return Ok(result);
        }

        // 📌 Add employee
        [HttpPost("add")]
        public async Task<IActionResult> AddEmployee([FromBody] AddEmployeeRequest request)
        {
            var result = await _context.EnhancedApiResponseDtos
                .FromSqlRaw("EXEC [ProjectManagement].[AddEmployee] @FullName, @Email, @Role",
                    new SqlParameter("@FullName", request.FullName ?? string.Empty),
                    new SqlParameter("@Email", request.Email ?? string.Empty),
                    new SqlParameter("@Role", request.Role ?? string.Empty))
                .ToListAsync();

            return Ok(result.FirstOrDefault());
        }
        [HttpPut("update")]
        public async Task<IActionResult> UpdateEmployee([FromBody] UpdateEmployeeRequest request)
        {
            var result = await _context.Set<apiResponseDto>()
                .FromSqlRaw(
                    "EXEC [ProjectManagement].[UpdateEmployee] @EmployeeID, @FullName, @Email, @Role",
                    new SqlParameter("@EmployeeID", request.EmployeeId),
                    new SqlParameter("@FullName", request.FullName ?? string.Empty),
                    new SqlParameter("@Email", request.Email ?? string.Empty),
                    new SqlParameter("@Role", request.Role ?? string.Empty)
                )
                .ToListAsync();

            return Ok(result.FirstOrDefault());
        }

        [HttpDelete("delete/{employeeId}")]
        public async Task<IActionResult> DeleteEmployee(int employeeId, [FromQuery] string userId = "SYSTEM")
        {
            var result = await _context.Set<apiResponseDto>()
                .FromSqlRaw(
                    "EXEC [ProjectManagement].[DeleteEmployee] @EmployeeID, @UserID",
                    new SqlParameter("@EmployeeID", employeeId),
                    new SqlParameter("@UserID", userId)
                )
                .ToListAsync();

            return Ok(result.FirstOrDefault());
        }

    }
}

namespace ProjectManagementApi.dTOs
{
    public class EnhancedApiResponsedTos<T>
    {
        public bool Success { get; set; }
        public string MessageType { get; set; } = "INFO";
        public string Message { get; set; } = string.Empty;

        [NotMapped]
        public T? Data { get; set; }
    }

    public class AddEmployeeRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // Request DTO for updating
    public class UpdateEmployeeRequest
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // Response DTO for queries
    public class EmployeesDto
    {
        public int EmployeeId { get; set; }
        public string TaskNames {  get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public int ActiveProjects { get; set; }
        public decimal TotalAllocation { get; set; }
        public int AssignedTasks { get; set; }
    }
    public class apiResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string MessageType { get; set; } = "INFO";
    }

}
