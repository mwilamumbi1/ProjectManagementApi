using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;
using ProjectManagementApi.DataContext;

namespace ProjectManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyProfileController : ControllerBase
    {
        private readonly PMDataContext _context;

        public CompanyProfileController(PMDataContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<List<PMCompanyProfileDto>>> GetProfile()
        {
            var profile = await _context.PMCompanyProfileDtos
                .FromSqlRaw("EXEC [ProjectManagement].[GetCompanyProfile]")
                .ToListAsync();

            return Ok(profile);
        }

        [HttpPost("add-profile")]
        public async Task<ActionResult<PMApiResponse>> AddProfile([FromBody] PMCompanyProfileRequest request)
        {
            var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 4000) { Direction = ParameterDirection.Output };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [ProjectManagement].[AddCompanyProfile] @Name, @Email, @Motto, @CompanyPhone, @PhysicalAddress, @PostalAddress, @EmailServerHost, @EmailServerPort, @EmailUsername, @EmailPassword, @UseSSL, @Success OUTPUT, @Message OUTPUT",
                new SqlParameter("@Name", request.CompanyName),
                new SqlParameter("@Email", request.CompanyEmail),
                new SqlParameter("@Motto", request.Motto),
                new SqlParameter("@CompanyPhone", (object)request.CompanyPhone ?? DBNull.Value),
                new SqlParameter("@PhysicalAddress", (object)request.PhysicalAddress ?? DBNull.Value),
                new SqlParameter("@PostalAddress", (object)request.PostalAddress ?? DBNull.Value),
                new SqlParameter("@EmailServerHost", (object)request.EmailServerHost ?? DBNull.Value),
                new SqlParameter("@EmailServerPort", (object)request.EmailServerPort ?? DBNull.Value),
                new SqlParameter("@EmailUsername", (object)request.EmailUsername ?? DBNull.Value),
                new SqlParameter("@EmailPassword", (object)request.EmailPassword ?? DBNull.Value),
                new SqlParameter("@UseSSL", (object)request.UseSSL ?? DBNull.Value),
                successParam,
                messageParam
            );

            return Ok(new PMApiResponse
            {
                Success = (bool)successParam.Value,
                Message = messageParam.Value?.ToString()
            });
        }

        [HttpPut("update-profile")]
        public async Task<ActionResult<PMApiResponse>> UpdateProfile([FromBody] PMCompanyProfileRequest request)
        {
            var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 4000) { Direction = ParameterDirection.Output };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [ProjectManagement].[UpdateCompanyProfile] @Name, @Email, @Motto, @CompanyPhone, @PhysicalAddress, @PostalAddress, @EmailServerHost, @EmailServerPort, @EmailUsername, @EmailPassword, @UseSSL, @ProfilePic, @Success OUTPUT, @Message OUTPUT",
                new SqlParameter("@Name", request.CompanyName),
                new SqlParameter("@Email", request.CompanyEmail),
                new SqlParameter("@Motto", request.Motto),
                new SqlParameter("@CompanyPhone", (object)request.CompanyPhone ?? DBNull.Value),
                new SqlParameter("@PhysicalAddress", (object)request.PhysicalAddress ?? DBNull.Value),
                new SqlParameter("@PostalAddress", (object)request.PostalAddress ?? DBNull.Value),
                new SqlParameter("@EmailServerHost", (object)request.EmailServerHost ?? DBNull.Value),
                new SqlParameter("@EmailServerPort", (object)request.EmailServerPort ?? DBNull.Value),
                new SqlParameter("@EmailUsername", (object)request.EmailUsername ?? DBNull.Value),
                new SqlParameter("@EmailPassword", (object)request.EmailPassword ?? DBNull.Value),
                new SqlParameter("@UseSSL", (object)request.UseSSL ?? DBNull.Value),
                new SqlParameter("@ProfilePic", SqlDbType.VarBinary, -1) { Value = DBNull.Value },
                successParam,
                messageParam
            );

            return Ok(new PMApiResponse
            {
                Success = (bool)successParam.Value,
                Message = messageParam.Value?.ToString()
            });
        }
    }

    // --- DTOs ---

    public class PMCompanyProfileDto
    {
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string Motto { get; set; }
        public string? CompanyPhone { get; set; }
        public string? PhysicalAddress { get; set; }
        public string? PostalAddress { get; set; }
        public string? EmailServerHost { get; set; }
        public int? EmailServerPort { get; set; }
        public string? EmailUsername { get; set; }
        public bool? UseSSL { get; set; }
        public string? EmailPassword { get; set; }

        // ✅ FIXED
        public byte[]? ProfilePic { get; set; }
    }

    public class PMCompanyProfileRequest
    {
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string Motto { get; set; }
        public string? CompanyPhone { get; set; }
        public string? PhysicalAddress { get; set; }
        public string? PostalAddress { get; set; }
        public string? EmailServerHost { get; set; }
        public int? EmailServerPort { get; set; }
        public string? EmailUsername { get; set; }
        public string? EmailPassword { get; set; }
        public bool? UseSSL { get; set; }
    }

    public class PMApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}