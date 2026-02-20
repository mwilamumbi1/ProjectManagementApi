using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.DataContext;


namespace ProjectManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourceAllocationController : ControllerBase
    {
        private readonly PMDataContext _context;

        public ResourceAllocationController(PMDataContext context)
        {
            _context = context;
        }

        // GET: api/ResourceAllocation
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllAllocations()
        {
            try
            {
                var allocations = await _context.ResourceAllocations
                    .FromSqlRaw("EXEC ProjectManagement.GetAllResourceAllocations")
                    .ToListAsync();

                return Ok(allocations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        // POST: api/ResourceAllocation/Add
        [HttpPost("Add")]
        public async Task<IActionResult> AddAllocation([FromBody] AddResourceAllocationDto dto)
        {
            try
            {
                var result = await _context.Set<SPResult>()
                    .FromSqlInterpolated($@"
                        EXEC ProjectManagement.AddResourceAllocation 
                        @ProjectID = {dto.ProjectID}, 
                        @EmployeeID = {dto.EmployeeID}, 
                        @Role = {dto.Role}, 
                        @AllocationPercentage = {dto.AllocationPercentage}, 
                        @StartDate = {dto.StartDate}, 
                        @EndDate = {dto.EndDate}")
                    .ToListAsync();

                return Ok(result.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        // PUT: api/ResourceAllocation/Update
        [HttpPut("Update")]
        public async Task<IActionResult> UpdateAllocation([FromBody] UpdateResourceAllocationDto dto)
        {
            try
            {
                var rows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
                    EXEC ProjectManagement.UpdateResourceAllocation
                    @AllocationID = {dto.AllocationID},
                    @Role = {dto.Role},
                    @AllocationPercentage = {dto.AllocationPercentage},
                    @StartDate = {dto.StartDate},
                    @EndDate = {dto.EndDate}");

                return Ok(new { Success = true, RowsAffected = rows });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        // DELETE: api/ResourceAllocation/Delete/{id}
        [HttpDelete("Delete/{allocationID}")]
        public async Task<IActionResult> DeleteAllocation(int allocationID, [FromQuery] string userID = "SYSTEM")
        {
            try
            {
                var result = await _context.Set<SPResult>()
                    .FromSqlInterpolated($@"
                        EXEC ProjectManagement.DeleteResourceAllocation 
                        @AllocationID = {allocationID}, 
                        @UserID = {userID}")
                    .ToListAsync();

                return Ok(result.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        // GET: api/ResourceAllocation/ValidateWorkload/{employeeID}
        [HttpGet("ValidateWorkload/{employeeID}")]
        public async Task<IActionResult> ValidateEmployeeWorkload(int employeeID)
        {
            try
            {
                var result = await _context.Set<EmployeeWorkloadResult>()
    .FromSqlInterpolated($@"
        EXEC ProjectManagement.ValidateEmployeeWorkload
        @EmployeeID = {employeeID}")
    .ToListAsync();

                return Ok(result.FirstOrDefault() ?? new EmployeeWorkloadResult
                {
                    EmployeeID = employeeID,
                    Success = false,
                    Message = "No data returned for this employee"
                });


                return Ok(result.FirstOrDefault());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

    }

    // DTOs for input
    public class ResourceAllocationDto
    {
        public int AllocationID { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal AllocationPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string AllocationStatus { get; set; } = string.Empty;
    }

    public class AddResourceAllocationDto
    {
        public int ProjectID { get; set; }
        public int EmployeeID { get; set; }
        public string? Role { get; set; }
        public decimal AllocationPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UpdateResourceAllocationDto
    {
        public int AllocationID { get; set; }
        public string? Role { get; set; }
        public decimal AllocationPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    // SP Result DTOs
    public class SPResult
    {
        public bool Success { get; set; }
        public string MessageType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Data { get; set; }
    }

    public class EmployeeWorkloadResult
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public decimal? TotalAllocation { get; set; }  // nullable
        public DateTime? CheckDate { get; set; }       // nullable
        public string AllocationStatus { get; set; } = string.Empty;
        public bool? Success { get; set; }            // nullable
        public string? MessageType { get; set; }      // nullable
        public string? Message { get; set; }          // nullable
        public string? Data { get; set; }             // nullable
    }

}

