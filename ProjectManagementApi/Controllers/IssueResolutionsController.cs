using Microsoft.EntityFrameworkCore;

namespace ProjectManagementApi.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using ProjectManagementApi.DataContext;
    using ProjectManagementApi.DTos;

    [Route("api/[controller]")]
    [ApiController]
    public class IssueResolutionsController : ControllerBase
    {
        private readonly PMDataContext _context;

        public IssueResolutionsController(PMDataContext context)
        {
            _context = context;
        }

        // GET: api/IssueResolutions/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllResolutions()
        {
            var resolutions = await _context.IssueResolutionDtos
                .FromSqlRaw("EXEC ProjectManagement.[GetAllIssueResolutions]")
                .ToListAsync();

            return Ok(resolutions);
        }

        // POST: api/IssueResolutions/Add
        [HttpPost("Add")]
        public async Task<IActionResult> AddResolution([FromBody] AddIssueResolutionDto dto)
        {
            var issueParam = new SqlParameter("@IssueID", dto.IssueID);
            var employeeParam = new SqlParameter("@EmployeeID", dto.EmployeeID);
            var notesParam = new SqlParameter("@ResolutionNotes", (object?)dto.ResolutionNotes ?? DBNull.Value);

            var result = await _context.GenericResponse
                .FromSqlRaw("EXEC ProjectManagement.AddIssueResolution @IssueID, @EmployeeID, @ResolutionNotes",
                    issueParam, employeeParam, notesParam)
                .ToListAsync();

            return Ok(result.FirstOrDefault());
        }

        // PUT: api/IssueResolutions/Update/{id}
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateResolution(int id, [FromBody] AddIssueResolutionDto dto)
        {
            var idParam = new SqlParameter("@ResolutionID", id);
            var employeeParam = new SqlParameter("@EmployeeID", dto.EmployeeID);
            var notesParam = new SqlParameter("@ResolutionNotes", (object?)dto.ResolutionNotes ?? DBNull.Value);

            var result = await _context.StoredProcedureResponse
                .FromSqlRaw("EXEC ProjectManagement.UpdateIssueResolution @ResolutionID, @ResolutionNotes, @EmployeeID",
                    idParam, notesParam, employeeParam)
                .ToListAsync();

            return Ok(result.FirstOrDefault());
        }

        // DELETE: api/IssueResolutions/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteResolution(int id)
        {
            var idParam = new SqlParameter("@ResolutionID", id);

            var result = await _context.GenericResponse
                .FromSqlRaw("EXEC ProjectManagement.DeleteIssueResolution @ResolutionID", idParam)
                .ToListAsync();

            return Ok(result.FirstOrDefault());
        }
    }
}

namespace ProjectManagementApi.DTos
{
    public class IssueResolutionDto
    {
        public int ResolutionID { get; set; }
        public int IssueID { get; set; }
        public string? IssueTitle { get; set; }    
        public int EmployeeID { get; set; }
        public string? FullName { get; set; }     
        public string? ResolutionNotes { get; set; }
        public DateTime ResolutionDate { get; set; }
    }


    public class AddIssueResolutionDto
    {
        public int IssueID { get; set; }
        public int EmployeeID { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    namespace ProjectManagementApi.DTos
    {
        [Keyless]
        public class StoredProcedureResponse
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}
