using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.DataContext;

[Route("api/[controller]")]
[ApiController]
public class MilestoneController : ControllerBase
{
    private readonly PMDataContext _context;

    public MilestoneController(PMDataContext context)
    {
        _context = context;
    }

    // GET: api/Milestone/GetAll
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var milestones = await _context.MilestoneDtos
                .FromSqlRaw("EXEC ProjectManagement.GetMilestones")
                .ToListAsync();

            return Ok(milestones);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }

    // POST: api/Milestone/Insert
    [HttpPost("Insert")]
    public async Task<IActionResult> Insert([FromBody] InsertMilestoneDto dto)
    {
        try
        {
            var successParam = new SqlParameter("@Success", System.Data.SqlDbType.Bit) { Direction = System.Data.ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", System.Data.SqlDbType.NVarChar, 4000) { Direction = System.Data.ParameterDirection.Output };

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC ProjectManagement.InsertMilestone
                    @ProjectID = {dto.ProjectID},
                    @MilestoneName = {dto.MilestoneName},
                    @Description = {dto.Description},
                    @DueDate = {dto.DueDate},
                    @CompletionDate = {dto.CompletionDate},
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

    // PUT: api/Milestone/Update
    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] UpdateMilestoneDto dto)
    {
        try
        {
            var successParam = new SqlParameter("@Success", System.Data.SqlDbType.Bit) { Direction = System.Data.ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", System.Data.SqlDbType.NVarChar, 4000) { Direction = System.Data.ParameterDirection.Output };

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC ProjectManagement.UpdateMilestone
                    @MilestoneID = {dto.MilestoneID},
                    @ProjectID = {dto.ProjectID},
                    @MilestoneName = {dto.MilestoneName},
                    @Description = {dto.Description},
                    @DueDate = {dto.DueDate},
                    @CompletionDate = {dto.CompletionDate},
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

    // DELETE: api/Milestone/Delete/{id}
    [HttpDelete("Delete/{milestoneID}")]
    public async Task<IActionResult> Delete(int milestoneID)
    {
        try
        {
            var successParam = new SqlParameter("@Success", System.Data.SqlDbType.Bit) { Direction = System.Data.ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", System.Data.SqlDbType.NVarChar, 4000) { Direction = System.Data.ParameterDirection.Output };

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC ProjectManagement.DeleteMilestone
                    @MilestoneID = {milestoneID},
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


public class MilestoneDto
{
    public int MilestoneID { get; set; }
    public int ProjectID { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string MilestoneName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? CompletionDate { get; set; }
}

public class InsertMilestoneDto
{
    public int ProjectID { get; set; }
    public string MilestoneName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? CompletionDate { get; set; }
}

public class UpdateMilestoneDto
{
    public int MilestoneID { get; set; }
    public int ProjectID { get; set; }
    public string MilestoneName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? CompletionDate { get; set; }
}

public class SPResultss
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
