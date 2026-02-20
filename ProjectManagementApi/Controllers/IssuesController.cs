using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.Controllers;
using ProjectManagementApi.DataContext;
using System.Data;

[Route("api/[controller]")]
[ApiController]
public class IssueController : ControllerBase
{
    private readonly PMDataContext _context;

    public IssueController(PMDataContext context)
    {
        _context = context;
    }

    // GET: api/Issue
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var issues = await _context.IssueDtos
                .FromSqlRaw("EXEC ProjectManagement.GetIssues")
                .ToListAsync();

            return Ok(issues);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }

    // POST: api/Issue/Insert
    [HttpPost("Insert")]
    public async Task<IActionResult> Insert([FromBody] InsertIssueDto dto)
    {
        try
        {
            var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 4000) { Direction = ParameterDirection.Output };

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC ProjectManagement.InsertIssue 
                    @ClientProjectID = {dto.ClientProjectID},
                    @IssueTitle = {dto.IssueTitle},
                    @IssueDescription = {dto.IssueDescription},
                 
                    @Success = {successParam} OUTPUT,
                    @Message = {messageParam} OUTPUT
            ");

            return Ok(new SPResult
            {
                Success = (bool)successParam.Value,
                Message = messageParam.Value?.ToString() ?? ""
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }

    // PUT: api/Issue/Update
    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] UpdateIssueDto dto)
    {
        try
        {
            var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 4000) { Direction = ParameterDirection.Output };

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC ProjectManagement.UpdateIssue
                    @IssueID = {dto.IssueID},
                    @ClientProjectID = {dto.ClientProjectID},
                    @IssueTitle = {dto.IssueTitle},
                    @IssueDescription = {dto.IssueDescription},
                    @Status = {dto.Status},
                    @ResolvedDate = {dto.ResolvedDate},
                    @Success = {successParam} OUTPUT,
                    @Message = {messageParam} OUTPUT
            ");

            return Ok(new SPResult
            {
                Success = (bool)successParam.Value,
                Message = messageParam.Value?.ToString() ?? ""
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }

    // DELETE: api/Issue/Delete/{id}
    [HttpDelete("Delete/{issueID}")]
    public async Task<IActionResult> Delete(int issueID)
    {
        try
        {
            var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 4000) { Direction = ParameterDirection.Output };

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC ProjectManagement.DeleteIssue
                    @IssueID = {issueID},
                    @Success = {successParam} OUTPUT,
                    @Message = {messageParam} OUTPUT
            ");

            return Ok(new SPResult
            {
                Success = (bool)successParam.Value,
                Message = messageParam.Value?.ToString() ?? ""
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }
}

// Input DTOs
public class InsertIssueDto
{
    public int ClientProjectID { get; set; }
    public string IssueTitle { get; set; } = string.Empty;
    public string IssueDescription { get; set; } = string.Empty;
 
}

public class UpdateIssueDto
{
    public int IssueID { get; set; }
    public int ClientProjectID { get; set; }
    public string IssueTitle { get; set; } = string.Empty;
    public string IssueDescription { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ResolvedDate { get; set; }
}

// For GetIssues SP
public class IssueDto
{
    public int? IssueID { get; set; }
    public int? ClientProjectID { get; set; }
    public string? ClientName { get; set; } = string.Empty;
    public string? ProjectName { get; set; } = string.Empty;
    public string? IssueTitle { get; set; } = string.Empty;
    public string? IssueDescription { get; set; } = string.Empty;
    public string? ResolvedBy { get; set; } = string.Empty;
    public string? Status { get; set; } = string.Empty;
    public DateTime? CreatedDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
}

// For output SPs (Insert, Update, Delete)
public class SPResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
