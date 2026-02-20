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
    public class BillingController : ControllerBase
    {
        private readonly PMDataContext _context;

        public BillingController(PMDataContext context)
        {
            _context = context;
        }

        [HttpGet("billings")]
        public async Task<ActionResult<List<BillingDto>>> GetAllBillings()
        {
            var billings = await _context.BillingDtos
                .FromSqlRaw("EXEC [ProjectManagement].[GetAllBillings]")
                .ToListAsync();

            return Ok(billings);
        }

        [HttpGet("billing-status")]
        public async Task<ActionResult<List<BillingStatusDto>>> GetAllBillingStatus()
        {
            var billingStatuses = await _context.BillingStatusDtos
                .FromSqlRaw("EXEC [ProjectManagement].[GetAllBillingStatus]")
                .ToListAsync();

            return Ok(billingStatuses);
        }

        [HttpPost("add-billing")]
        public async Task<ActionResult<EnhancedApiResponse>> AddBilling([FromBody] AddBillingRequest request)
        {
            var result = _context.EnhancedApiResponseDtos
                .FromSqlRaw(
                    "EXEC [ProjectManagement].[AddBilling] @ProjectID, @InvoiceNumber, @Amount, @BillingDate, @DueDate, @BillingStatusID",
                    new SqlParameter("@ProjectID", request.ProjectID),
                    new SqlParameter("@InvoiceNumber", request.InvoiceNumber),
                    new SqlParameter("@Amount", request.Amount),
                    new SqlParameter("@BillingDate", request.BillingDate),
                    new SqlParameter("@DueDate", request.DueDate),
                    new SqlParameter("@BillingStatusID", (object)request.BillingStatusID ?? DBNull.Value)
                )
                .AsEnumerable() // 👈 fix
                .FirstOrDefault();

            if (result == null)
            {
                return BadRequest(new EnhancedApiResponse
                {
                    Success = false,
                    MessageType = "ERROR",
                    Message = "Failed to execute billing operation"
                });
            }

            if (result.Success)
                return Ok(result);
            else if (result.MessageType == "WARNING")
                return Conflict(result);
            else
                return BadRequest(result);
        }
        [HttpPut("change-status")]
        public async Task<ActionResult<EnhancedApiResponse>> ChangeBillingStatus([FromBody] ChangeBillingStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new EnhancedApiResponse
                {
                    Success = false,
                    MessageType = "ERROR",
                    Message = "Invalid request data"
                });
            }

            var responseParam = new SqlParameter
            {
                ParameterName = "@ResponseMessage",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Size = 250,
                Direction = System.Data.ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC ProjectManagement.ChangeBillingStatus @BillingID, @NewStatusID, @ResponseMessage OUTPUT",
                new SqlParameter("@BillingID", request.BillingID),
                new SqlParameter("@NewStatusID", request.NewStatusID),
                responseParam
            );

            return Ok(new EnhancedApiResponse
            {
                Success = true,
                
                Message = responseParam.Value.ToString()
            });
        }


        [HttpPut("update-billing")]
        public async Task<ActionResult<UpdateBillingResponse>> UpdateBilling([FromBody] UpdateBillingRequest request)
        {
            var result = _context.UpdateBillingResponseDtos
                .FromSqlRaw(
                    "EXEC [ProjectManagement].[UpdateBilling] @BillingID, @Amount, @BillingDate, @DueDate, @BillingStatusID",
                    new SqlParameter("@BillingID", request.BillingID),
                    new SqlParameter("@Amount", request.Amount),
                    new SqlParameter("@BillingDate", request.BillingDate),
                    new SqlParameter("@DueDate", request.DueDate),
                    new SqlParameter("@BillingStatusID", (object)request.BillingStatusID ?? DBNull.Value)
                )
                .AsEnumerable() // 👈 fix
                .FirstOrDefault();

            if (result == null)
            {
                return BadRequest(new UpdateBillingResponse { RowsAffected = 0 });
            }

            return Ok(result);
        }

        [HttpDelete("delete-billing/{billingId}")]
        public async Task<ActionResult<EnhancedApiResponse>> DeleteBilling(int billingId, [FromQuery] string userId = "SYSTEM")
        {
            var result = _context.EnhancedApiResponseDtos
                .FromSqlRaw(
                    "EXEC [ProjectManagement].[DeleteBilling] @BillingID, @UserID",
                    new SqlParameter("@BillingID", billingId),
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
        // DTOs - Add these to your ProjectManagementApi.Dtos namespace
        public class BillingDto
    {
        public int BillingID { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime DueDate { get; set; }
        public int? BillingStatusID { get; set; }
        public string? BillingStatus { get; set; }
        public int DaysUntilDue { get; set; }
    }

    public class BillingStatusDto
    {
        public int BillingStatusID { get; set; }
        public string StatusName { get; set; }
    }

    public class ChangeBillingStatusRequest
    {
        [Required]
        public int BillingID { get; set; }

        [Required]
        public int NewStatusID { get; set; }
    }
    public class AddBillingRequest
    {
        public int ProjectID { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime DueDate { get; set; }
        public int? BillingStatusID { get; set; }
    }

    public class UpdateBillingRequest
    {
        public int BillingID { get; set; }
        public decimal Amount { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime DueDate { get; set; }
        public int? BillingStatusID { get; set; }
    }

    public class EnhancedApiResponse
    {
        public bool Success { get; set; }
        public string MessageType { get; set; }
        public string Message { get; set; }
        public string? Data { get; set; }
    }

    public class UpdateBillingResponse
    {
        public int RowsAffected { get; set; }
    }
}