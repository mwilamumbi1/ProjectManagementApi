using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.DataContext;
using ProjectManagementApi.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace ProjectManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetController : ControllerBase
    {
        private readonly PMDataContext _context;

        public BudgetController(PMDataContext context)
        {
            _context = context;
        }

        [HttpGet("budgets")]
        public async Task<ActionResult<List<BudgetDto>>> GetAllBudgets()
        {
            var budgets = await _context.BudgetDtos
                .FromSqlRaw("EXEC [ProjectManagement].[GetAllBudgets]")
                .ToListAsync();

            return Ok(budgets);
        }
        [HttpPost("add-budget")]
        public async Task<ActionResult<EnhancedApiResponse>> AddBudget([FromBody] AddBudgetRequest request)
        {
            var result = (await _context.EnhancedApiResponseDtos
                .FromSqlRaw(
                    "EXEC [ProjectManagement].[AddBudget] @ProjectID, @EstimatedCost, @ActualCost,  @ApprovedDate",
                    new SqlParameter("@ProjectID", request.ProjectID),
                    new SqlParameter("@EstimatedCost", request.EstimatedCost),
                    new SqlParameter("@ActualCost", request.ActualCost ?? 0), // Added this line
                    
                    new SqlParameter("@ApprovedDate", (object)request.ApprovedDate ?? DBNull.Value)
                )
                .ToListAsync())
                .FirstOrDefault();

            if (result == null)
            {
                return BadRequest(new EnhancedApiResponse
                {
                    Success = false,
                    MessageType = "ERROR",
                    Message = "Failed to execute budget operation"
                });
            }

            if (result.Success)
                return Ok(result);
            else if (result.MessageType == "WARNING")
                return Conflict(result);
            else
                return BadRequest(result);
        }

        [HttpPut("update-budget")]
        public async Task<ActionResult<UpdateBudgetResponse>> UpdateBudget([FromBody] UpdateBudgetRequest request)
        {
            var result = await _context.UpdateBudgetResponseDtos
                .FromSqlRaw(
                    "EXEC [ProjectManagement].[UpdateBudget] @BudgetID, @EstimatedCost, @ActualCost,  @ApprovedDate",
                    new SqlParameter("@BudgetID", request.BudgetID),
                    new SqlParameter("@EstimatedCost", request.EstimatedCost),
                    new SqlParameter("@ActualCost", request.ActualCost ?? 0),
                    
                    new SqlParameter("@ApprovedDate", (object)request.ApprovedDate ?? DBNull.Value)
                )
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return BadRequest(new UpdateBudgetResponse { RowsAffected = 0 });
            }

            return Ok(result);
        }

        [HttpDelete("delete-budget/{budgetId}")]
        public async Task<ActionResult<EnhancedApiResponse>> DeleteBudget(int budgetId, [FromQuery] int? userId = null)
        {
            var results = await _context.Set<DeleteBudgetResult>()
      .FromSqlRaw(
          "EXEC [ProjectManagement].[DeleteBudget] @BudgetID, @USERID",
          new SqlParameter("@BudgetID", budgetId),
          new SqlParameter("@USERID", userId ?? 0))
      .ToListAsync();

            var result = results.SingleOrDefault();

 

            if (result == null)
            {
                return BadRequest(new EnhancedApiResponse
                {
                    Success = false,
                    MessageType = "ERROR",
                    Message = "Failed to execute delete operation"
                });
            }

            if (result.Success)
                return Ok(result);
            else if (result.MessageType == "WARNING")
                return NotFound(result);
            else
                return BadRequest(result);
        }
    }
        // DTOs - Add these to your ProjectManagementApi.Dtos namespace
        public class BudgetDto
    {
        public int BudgetID { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal ActualCost { get; set; }
       
        public DateTime? ApprovedDate { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public int CostItemsCount { get; set; }
    }

    public class AddBudgetRequest
    {
        public int ProjectID { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; } = 0;  // <-- Add this
       
        public DateTime? ApprovedDate { get; set; }
    }

    public class DeleteBudgetResult
    {
        public bool Success { get; set; }
        public string MessageType { get; set; }
        public string Message { get; set; }
    }



    public class UpdateBudgetRequest
    {
        public int BudgetID { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; } = 0;
        
        public DateTime? ApprovedDate { get; set; }
    }

    public class UpdateBudgetResponse
    {
        public int RowsAffected { get; set; }
    }


}