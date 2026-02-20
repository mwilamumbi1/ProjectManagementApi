using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.DataContext;
using ProjectManagementApi.Dtos;
using ProjectManagementApi.DTos;

namespace ProjectManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly PMDataContext _context;

        public PortfolioController(PMDataContext context)
        {
            _context = context;
        }

        // GET: api/Portfolio
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllPortfolios()
        {
            var portfolios = await _context.PortfolioDtos
                .FromSqlRaw("EXEC ProjectManagement.GetAllPortfolios")
                .ToListAsync();
            return Ok(portfolios);
        }

        // GET: api/Portfolio/{id}/Projects
        [HttpGet("{id}/Projects")]
        public async Task<IActionResult> GetPortfolioProjects(int id)
        {
            var projects = await _context.PortfolioProjectDtos
                .FromSqlRaw("EXEC ProjectManagement.GetPortfolioProjects @PortfolioID",
                    new SqlParameter("@PortfolioID", id))
                .ToListAsync();
            return Ok(projects);
        }

        // POST: api/Portfolio/Add
        [HttpPost("Add")]
        public async Task<IActionResult> AddPortfolio([FromBody] AddPortfolioDto dto)
        {
            var portfolioNameParam = new SqlParameter("@PortfolioName", dto.PortfolioName);
            var descriptionParam = new SqlParameter("@Description", (object?)dto.Description ?? DBNull.Value);
            var managerParam = new SqlParameter("@ManagerID", (object?)dto.ManagerID ?? DBNull.Value);

            var result = await _context.GenericResponse
                .FromSqlRaw("EXEC ProjectManagement.AddPortfolio @PortfolioName, @Description, @ManagerID",
                    portfolioNameParam, descriptionParam, managerParam)
                .ToListAsync();

            return Ok(result.FirstOrDefault());
        }

        // PUT: api/Portfolio/Update/{id}
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdatePortfolio(int id, [FromBody] UpdatePortfolioDto dto)
        {
            var portfolioIdParam = new SqlParameter("@PortfolioID", id);
            var portfolioNameParam = new SqlParameter("@PortfolioName", dto.PortfolioName);
            var descriptionParam = new SqlParameter("@Description", (object?)dto.Description ?? DBNull.Value);
            var managerParam = new SqlParameter("@ManagerID", (object?)dto.ManagerID ?? DBNull.Value);

            var rowsAffected = await _context.RowsAffectedDtos
                .FromSqlRaw("EXEC ProjectManagement.UpdatePortfolio @PortfolioID, @PortfolioName, @Description, @ManagerID",
                    portfolioIdParam, portfolioNameParam, descriptionParam, managerParam)
                .ToListAsync();

            return Ok(rowsAffected.FirstOrDefault());
        }

        // DELETE: api/Portfolio/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeletePortfolio(int id, string userId = "SYSTEM")
        {
            var portfolioIdParam = new SqlParameter("@PortfolioID", id);
            var userIdParam = new SqlParameter("@UserID", userId);

            var result = await _context.GenericResponse
                .FromSqlRaw("EXEC ProjectManagement.DeletePortfolio @PortfolioID, @UserID",
                    portfolioIdParam, userIdParam)
                .ToListAsync();

            return Ok(result.FirstOrDefault());
        }
    }
}


namespace ProjectManagementApi.DTos
{
    // For GetAllPortfolios
    public class PortfolioDto
    {
        public int PortfolioID { get; set; }
        public string PortfolioName { get; set; } = null!;
        public string? Description { get; set; }
        public int? ManagerID { get; set; }
        public string? ManagerName { get; set; }
        public int ProjectCount { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public decimal TotalActualCost { get; set; }
    }

    // For GetPortfolioProjects
    public class PortfolioProjectDto
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? StatusName { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
    }

    // Generic SP response (Add / Delete)
    public class GenericResponse
    {
        public bool Success { get; set; }
        public string MessageType { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Data { get; set; }
    }

    // Update SP response (rows affected)
    public class RowAffectedDto
    {
        public int RowsAffected { get; set; }
    }

    // AddPortfolio input DTO
    public class AddPortfolioDto
    {
        public string PortfolioName { get; set; } = null!;
        public string? Description { get; set; }
        public int? ManagerID { get; set; }
    }

    // UpdatePortfolio input DTO
    public class UpdatePortfolioDto
    {
        public string PortfolioName { get; set; } = null!;
        public string? Description { get; set; }
        public int? ManagerID { get; set; }
    }
}
