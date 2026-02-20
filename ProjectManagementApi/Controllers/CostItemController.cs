
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore; 
using ProjectManagementApi.DataContext;
using ProjectManagementApi.Dtos;
using System.Data;

namespace ProjectManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CostItemController : ControllerBase
    {
        private readonly PMDataContext _context;

        public CostItemController(PMDataContext context)
        {
            _context = context;
        }

        [HttpGet("cost-items")]
        public async Task<ActionResult<List<CostItemDto>>> GetAllCostItems()
        {
            var costItems = await _context.CostItemDtos
                .FromSqlRaw("EXEC [ProjectManagement].[GetAllCostItems]")
                .ToListAsync();

            return Ok(costItems);
        }

        [HttpGet("cost-items-by-budget/{budgetId}")]
        public async Task<ActionResult<List<CostItemByBudgetDto>>> GetCostItemsByBudget(int budgetId)
        {
            var costItems = await _context.CostItemByBudgetDtos
                .FromSqlRaw(
                    "EXEC [ProjectManagement].[GetCostItemsByBudget] @BudgetID",
                    new SqlParameter("@BudgetID", budgetId)
                )
                .ToListAsync();

            return Ok(costItems);
        }

        [HttpPost("add-cost-item")]
        public async Task<ActionResult<EnhancedApiResponse>> AddCostItem([FromBody] AddCostItemRequest request)
        {
            var result = _context.EnhancedApiResponseDtos
                .FromSqlRaw(
                    "EXEC [ProjectManagement].[AddCostItem] @BudgetID, @Description, @Amount, @DateIncurred",
                    new SqlParameter("@BudgetID", request.BudgetID),
                    new SqlParameter("@Description", request.Description),
                    new SqlParameter("@Amount", request.Amount),
                    new SqlParameter("@DateIncurred", request.DateIncurred)
                )
                .AsEnumerable() // 👈 fix
                .FirstOrDefault();

            if (result == null)
            {
                return BadRequest(new EnhancedApiResponse
                {
                    Success = false,
                    MessageType = "ERROR",
                    Message = "Failed to execute cost item operation"
                });
            }

            if (result.Success)
                return Ok(result);
            else if (result.MessageType == "WARNING")
                return Conflict(result);
            else
                return BadRequest(result);
        }

        [HttpPut("update-cost-item")]
        public async Task<ActionResult<UpdateCostItemResponse>> UpdateCostItem([FromBody] UpdateCostItemRequest request)
        {
            var result = _context.UpdateCostItemResponseDtos
                .FromSqlRaw(
                    "EXEC [ProjectManagement].[UpdateCostItem] @CostID, @Description, @Amount, @DateIncurred",
                    new SqlParameter("@CostID", request.CostID),
                    new SqlParameter("@Description", request.Description),
                    new SqlParameter("@Amount", request.Amount),
                    new SqlParameter("@DateIncurred", request.DateIncurred)
                )
                .AsEnumerable() // 👈 fix
                .FirstOrDefault();

            if (result == null)
            {
                return BadRequest(new UpdateCostItemResponse { RowsAffected = 0 });
            }

            return Ok(result);
        }

        [HttpDelete("delete-cost-item/{costId}")]
        public async Task<ActionResult<EnhancedApiResponse>> DeleteCostItem(int costId, [FromQuery] string userId = "SYSTEM")
        {
            var result = _context.EnhancedApiResponseDtos
                .FromSqlRaw(
                    "EXEC [ProjectManagement].[DeleteCostItem] @CostID, @UserID",
                    new SqlParameter("@CostID", costId),
                    new SqlParameter("@UserID", userId)
                )
                .AsEnumerable() // 👈 fix
                .FirstOrDefault();

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
}

    namespace ProjectManagementApi.Dtos
{
    public class CostItemDto
    {
        public int CostID { get; set; }
        public int BudgetID { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DateIncurred { get; set; }
    }

    public class CostItemByBudgetDto
    {
        public int CostID { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DateIncurred { get; set; }
    }

    public class AddCostItemRequest
    {
        public int BudgetID { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DateIncurred { get; set; }
    }

    public class UpdateCostItemRequest
    {
        public int CostID { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DateIncurred { get; set; }
    }

    public class UpdateCostItemResponse
    {
        public int RowsAffected { get; set; }
    }
}