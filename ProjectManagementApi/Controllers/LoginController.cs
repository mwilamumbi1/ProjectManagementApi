using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProjectManagementApi.DataContext;
using ProjectManagementApi.dTOs;
using ProjectManagementApi.DTOs;
using ProjectManagementApi.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjectManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly PMDataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public LoginController(PMDataContext context, IConfiguration configuration, IUserService userService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet("LOGS")]
        public async Task<IActionResult> GetAllLogs()
        {
            // NOTE: Ensure GetAuditLogDto is configured as a keyless entity in your OnModelCreating if you plan to map it via EF.
            var logs = await _context.Set<GetAuditLogDto>()
                                     .FromSqlRaw("EXEC ProjectManagement.GetAuditTrail")
                                     .ToListAsync();
            return Ok(logs);
        }
        [HttpPost("LoginPM")]
        public async Task<IActionResult> LoginPM([FromBody] LoginDto loginDto)
        {
            if (loginDto == null ||
                string.IsNullOrWhiteSpace(loginDto.Email) ||
                string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest(new { success = false, message = "Email and password are required." });
            }

            DbDataReader? reader = null;
            DbCommand? command = null;

            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection == null)
                {
                    return StatusCode(500, new { success = false, message = "Database connection is not available." });
                }

                var emailParam = new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = loginDto.Email };

                var passwordBytes = HashHelper.ComputeSha256Hash(loginDto.Password);
                var passwordParam = new SqlParameter("@Password", SqlDbType.VarBinary, 64)
                {
                    Value = passwordBytes
                };

                command = connection.CreateCommand();
                command.CommandText = "core.LoginPM"; // call the new PM procedure
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(emailParam);
                command.Parameters.Add(passwordParam);

                if (connection.State != ConnectionState.Open)
                {
                    await _context.Database.OpenConnectionAsync();
                }

                reader = await command.ExecuteReaderAsync();
                if (reader == null)
                {
                    return StatusCode(500, new { success = false, message = "Failed to execute stored procedure. Reader is null." });
                }

                var users = new List<UserResponseDto>();
                if (!reader.HasRows)
                {
                    return Unauthorized(new { success = false, message = "Invalid email or password." });
                }

                while (await reader.ReadAsync())
                {
                    users.Add(new UserResponseDto
                    {
                        UserID = GetSafeInt32(reader, "UserID"),
                        FullName = GetSafeString(reader, "FullName"),
                        Email = GetSafeString(reader, "Email"),
                        RoleId = GetSafeInt32(reader, "RoleId"),
                        RoleName = GetSafeString(reader, "RoleName"),
                        StatusId = GetSafeInt32(reader, "StatusId"),
                        CreatedAt = GetSafeDateTime(reader, "CreatedAt"),
                        PasswordExpiryDate = GetSafeNullableDateTime(reader, "PasswordExpiryDate"),
                        ComplexityId = GetSafeInt32(reader, "ComplexityId"),
                        CreatedBy = GetSafeString(reader, "CreatedBy"),
                        EmployeeID = GetSafeNullableInt32(reader, "EmployeeID"),
                        ClientID = GetSafeNullableInt32(reader, "ClientID"),
                        ClientName = GetSafeString(reader, "ClientName")
                        // Optionally, add PM role from stored procedure
                        // PMRole = GetSafeString(reader, "PMRole")
                    });
                }

                if (users.Count == 0)
                    return Unauthorized(new { success = false, message = "Invalid email or password." });

                var user = users.First();

                // Read permissions from next result set
                var permissions = new List<string>();
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        permissions.Add(GetSafeString(reader, "PermissionName"));
                    }
                }

                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    success = true,
                    token,
                    user,
                    permissions
                });
            }
            catch (SqlException sqlEx)
            {
                return StatusCode(500, new { success = false, message = $"Database error: {sqlEx.Message}", errorCode = sqlEx.Number });
            }
            catch (InvalidOperationException ioEx)
            {
                return StatusCode(500, new { success = false, message = $"Operation error: {ioEx.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"An error occurred: {ex.Message}", stackTrace = ex.StackTrace });
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    await reader.DisposeAsync();
                }

                if (command != null)
                {
                    command.Dispose();
                }

                if (_context.Database.GetDbConnection().State == ConnectionState.Open)
                {
                    await _context.Database.CloseConnectionAsync();
                }
            }
        }


        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { success = false, message = "All fields are required." });
            }

            var passwordHash = HashHelper.ComputeSha256Hash(dto.Password);

            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "core.AddUser_SignUp";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@FullName", dto.FullName));
                command.Parameters.Add(new SqlParameter("@Email", dto.Email));
                command.Parameters.Add(new SqlParameter("@PasswordHash", passwordHash));
                command.Parameters.Add(new SqlParameter("@CreatedBy", "SELF"));

                var userIdParam = new SqlParameter("@UserID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(userIdParam);

                await command.ExecuteNonQueryAsync();

                return Ok(new
                {
                    success = true,
                    message = "Account created successfully. Awaiting activation.",
                    userId = (int)userIdParam.Value
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(400, new { success = false, message = ex.Message });
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }


        [HttpGet("inactiveUsers")]
        public async Task<IActionResult> GetInactiveNonEmployeeUsers()
        {
            try
            {
                var users = await _context.Set<InactiveUserDto>()
                    .FromSqlRaw("EXEC core.usp_GetInactiveNonEmployeeUsers")
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    count = users.Count,
                    data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to retrieve inactive users.",
                    error = ex.Message
                });
            }
        }

        // --- Link user to employee & send activation email ---
        [HttpPost("linkUserToEmployee")]
        public async Task<IActionResult> LinkUserToEmployee([FromBody] LinkUserDto dto)
        {
            if (dto.UserID <= 0 || string.IsNullOrWhiteSpace(dto.Role))
                return BadRequest(new { success = false, message = "UserID and Role are required." });

            try
            {
                // 1️⃣ Link user
                var result = await _context.Set<EmployeeDto>()
                    .FromSqlRaw("EXEC ProjectManagement.usp_LinkUserToEmployee @UserID = {0}, @Role = {1}", dto.UserID, dto.Role)
                    .AsNoTracking()
                    .ToListAsync();

                if (!result.Any())
                    return NotFound(new { success = false, message = "Failed to link user to employee." });

                // 2️⃣ Get the linked user's details
                var user = result.First();

                // 3️⃣ Get company profile
                var companyProfile = await _context.PMCompanyProfileDtos.FirstOrDefaultAsync();
                var fromEmail = companyProfile?.CompanyEmail ?? "no-reply@company.com";

                // 4️⃣ Send email
                try
                {
                    var smtp = new System.Net.Mail.SmtpClient
                    {
                        Host = companyProfile?.EmailServerHost ?? "smtp.server.com",
                        Port = companyProfile?.EmailServerPort ?? 25,
                        EnableSsl = companyProfile?.UseSSL ?? false,
                        Credentials = new System.Net.NetworkCredential(
                            companyProfile?.EmailUsername ?? "username",
                            companyProfile?.EmailPassword ?? "password")
                    };

                    var mail = new System.Net.Mail.MailMessage
                    {
                        From = new System.Net.Mail.MailAddress(fromEmail, companyProfile?.CompanyName ?? "Company"),
                        Subject = "Your account has been activated",
                        Body = $"Hello {user.FullName},\n\nYour account has been successfully activated.\n\nBest regards,\n{companyProfile?.CompanyName}",
                        IsBodyHtml = false
                    };
                    mail.To.Add(user.Email);
                    smtp.Send(mail);
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"Email sending failed: {emailEx.Message}");
                }

                return Ok(new { success = true, message = "User linked to employee successfully and email sent.", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to link user to employee.", error = ex.Message });
            }
        }
        [HttpDelete("deleteInactiveUser/{userId}")]
        public async Task<IActionResult> DeleteInactiveUser(int userId)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC core.usp_DeleteInactiveUser @UserID = {0}", userId);

                return Ok(new
                {
                    success = true,
                    message = "User deleted successfully.",
                    deletedUserId = userId
                });
            }
            catch (SqlException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // **NEW ENDPOINT**
        [Authorize]
        [HttpGet("Permissions")]
        public async Task<IActionResult> GetPermissions()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("User ID claim not found or invalid.");
            }

            var permissions = await _userService.GetPermissionsByUserIdAsync(userId);

            if (permissions == null || !permissions.Any())
            {
                return NotFound("No permissions found for this user.");
            }

            return Ok(permissions);
        }

        // Helper methods
        private static int GetSafeInt32(DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return 0;
            }
        }

        private static int? GetSafeNullableInt32(DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        private static string GetSafeString(DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return string.Empty;
            }
        }

        private static DateTime GetSafeDateTime(DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? DateTime.MinValue : reader.GetDateTime(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return DateTime.MinValue;
            }
        }

        private static DateTime? GetSafeNullableDateTime(DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        private string GenerateJwtToken(UserResponseDto user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationHours = int.TryParse(jwtSettings["ExpirationInHours"], out var h) ? h : 24;

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured");
            }

            var key = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Role, user.RoleName ?? string.Empty),
                    new Claim("RoleId", user.RoleId.ToString()),
                    new Claim("StatusId", user.StatusId.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(expirationHours),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    // ---- Helper classes (move to separate files if desired) ----

    public static class HashHelper
    {
        public static byte[] ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(rawData);
                return sha256.ComputeHash(inputBytes);
            }
        }

        public static string ComputeSha256HashHex(string rawData)
        {
            var hash = ComputeSha256Hash(rawData);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public static bool VerifyPassword(string plainTextPassword, byte[] storedHash)
        {
            var computedHash = ComputeSha256Hash(plainTextPassword);
            return computedHash.SequenceEqual(storedHash);
        }

        // ✅ REPLACE THE STUB WITH THIS IMPLEMENTATION
        public static byte[] ComputeSha256HashBytes(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(rawData);
                return sha256.ComputeHash(inputBytes);
            }
        }
    }

    public class LoginDto
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    public class UserResponseDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PasswordExpiryDate { get; set; }
        public int ComplexityId { get; set; }
        public string? CreatedBy { get; set; }
        public int? EmployeeID { get; set; }
        public int? ClientID { get; set; }
        public string? ClientName { get; set; }

    }

    public class GetAuditLogDto
    {
        public long AuditID { get; set; }            // bigint → long
        public string TableName { get; set; } = string.Empty;
        public string PrimaryKeyValue { get; set; } = string.Empty; // nvarchar(100) → string
        public string ActionType { get; set; } = string.Empty;
        public string? OldData { get; set; }         // nvarchar(max) → string?
        public string? NewData { get; set; }         // nvarchar(max) → string?
        public string UserID { get; set; } = string.Empty; // nvarchar(150) → string
        public DateTime ActionDate { get; set; }

        // Extra fields (from JOINs, not in table)
        //public string? RoleName { get; set; }
       // public string? EmployeeName { get; set; }
    }

    public class LinkUserDto
    {
        public int UserID { get; set; }
        public string Role { get; set; }
    }

    public class DeleteUserResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? DeletedUserID { get; set; }
    }


    public class EmployeeDto
    {
        public int UserID { get; set; }
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    
    }

    public class InactiveUserDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }


    public class AddUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public int CreatedBy { get; set; }
        public int? StatusId { get; set; }
    }

    public class SignUpDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

}
