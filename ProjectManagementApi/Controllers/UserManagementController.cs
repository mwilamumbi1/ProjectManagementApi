using Core.HumanResourceManagementApi.DTOs;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.DataContext;
using ProjectManagementApi.Helpers;
using ProjectManagementApi.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;

namespace ProjectManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserManagementController : ControllerBase
    {
        private readonly PMDataContext _context;

        public UserManagementController(PMDataContext context)
        {
            _context = context;
        }

        // ========================= ROLES =========================
        [HttpGet("GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _context.Set<GetAllRolesDto>()
                    .FromSqlRaw("EXEC core.GetRoles")
                    .ToListAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("AddRole")]
        public async Task<IActionResult> AddRole([FromBody] AddRoleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC core.AddRole @RoleName, @Description",
                    new SqlParameter("@RoleName", dto.RoleName),
                    new SqlParameter("@Description", (object?)dto.Description ?? DBNull.Value)
                );

                return Ok(new ResponseDto { Success = true, Message = "Role added successfully." });
            }
            catch (SqlException ex)
            {
                return BadRequest(new ResponseDto { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("UpdateRole")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC core.UpdateRole @RoleID, @RoleName, @Description",
                    new SqlParameter("@RoleID", dto.RoleID),
                    new SqlParameter("@RoleName", dto.RoleName),
                    new SqlParameter("@Description", (object?)dto.Description ?? DBNull.Value)
                );

                return Ok(new ResponseDto { Success = true, Message = "Role updated successfully." });
            }
            catch (SqlException ex)
            {
                return BadRequest(new ResponseDto { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("DeleteRole")]
        public async Task<IActionResult> DeleteRole([FromBody] DeleteRoleDto dto)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC core.DeleteRole @RoleID",
                    new SqlParameter("@RoleID", dto.RoleID)
                );

                return Ok(new ResponseDto { Success = true, Message = "Role deleted successfully." });
            }
            catch (SqlException ex)
            {
                return BadRequest(new ResponseDto { Success = false, Message = ex.Message });
            }
        }

        // ========================= EMPLOYEES + CLIENTS =========================
        [HttpGet("GetAllEmployeesAndClients")]
        public async Task<IActionResult> GetAllEmployeesAndClients()
        {
            try
            {
                var result = await _context.Set<EmployeeClientDto>()
                    .FromSqlRaw("EXEC ProjectManagement.GetAllEmployeesAndClients")
                    .ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto { Success = false, Message = ex.Message });
            }
        }

        // ========================= ADD USER =========================
        [HttpPost("AddUser")]
        public async Task<IActionResult> AddUser([FromBody] AddPMUserDto dto)
        {
            if (dto.RoleId <= 0 || dto.CreatedBy <= 0)
                return BadRequest(new { success = false, message = "RoleId and CreatedBy are required." });

            string generatedPassword = PasswordGenerator.GenerateRandomPassword();
            var passwordHash = HashHelper.ComputeSha256Hash(generatedPassword);

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            int newUserId;

            // ===================== DB TRANSACTION =====================
            using var transaction = connection.BeginTransaction();
            try
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "core.AddPMUser";

                command.Parameters.Add(new SqlParameter("@EmployeeID", (object?)dto.EmployeeID ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@ClientID", (object?)dto.ClientID ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@PasswordHash", passwordHash));
                command.Parameters.Add(new SqlParameter("@RoleId", dto.RoleId));
                command.Parameters.Add(new SqlParameter("@CreatedBy", dto.CreatedBy));
                command.Parameters.Add(new SqlParameter("@StatusId", (object?)dto.StatusId ?? DBNull.Value));

                var userIdParam = new SqlParameter("@UserID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(userIdParam);

                await command.ExecuteNonQueryAsync();

                newUserId = (int)userIdParam.Value;

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, new { success = false, message = ex.Message });
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }

            // ===================== SEND EMAIL =====================
            bool emailSent = false;
            string emailError = null;

            try
            {
                // Get user details
                string getUserQuery;
                if (dto.EmployeeID.HasValue && dto.EmployeeID > 0)
                {
                    getUserQuery = "SELECT FullName, Email FROM ProjectManagement.Employees WHERE UserID = @UserID";
                }
                else if (dto.ClientID.HasValue && dto.ClientID > 0)
                {
                    getUserQuery = "SELECT ClientName AS FullName, ContactEmail AS Email FROM ProjectManagement.Clients WHERE UserID = @UserID";
                }
                else
                {
                    throw new Exception("Neither EmployeeID nor ClientID was provided");
                }

                string fullName = null;
                string email = null;

                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = getUserQuery;
                    cmd.Parameters.Add(new SqlParameter("@UserID", newUserId));

                    if (cmd.Connection.State != ConnectionState.Open)
                        await cmd.Connection.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            fullName = reader["FullName"]?.ToString();
                            email = reader["Email"]?.ToString();
                        }
                    }
                }

                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email))
                {
                    throw new Exception($"Could not find user with UserID {newUserId}");
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new Exception("User email is empty or invalid");
                }

                // Get company profile
                PMCompanyProfileDto company = null;

                using (var cmd = _context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT TOP 1 
                            CompanyName,
                            CompanyEmail,
                            EmailServerHost,
                            EmailServerPort,
                            EmailUsername,
                            EmailPassword,
                            UseSSL
                        FROM ProjectManagement.CompanyProfile";

                    if (cmd.Connection.State != ConnectionState.Open)
                        await cmd.Connection.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            company = new PMCompanyProfileDto
                            {
                                CompanyName = reader["CompanyName"]?.ToString(),
                                CompanyEmail = reader["CompanyEmail"]?.ToString(),
                                EmailServerHost = reader["EmailServerHost"]?.ToString(),
                                EmailServerPort = reader["EmailServerPort"] != DBNull.Value ? (int?)reader["EmailServerPort"] : null,
                                EmailUsername = reader["EmailUsername"]?.ToString(),
                                EmailPassword = reader["EmailPassword"]?.ToString(),
                                UseSSL = reader["UseSSL"] != DBNull.Value ? (bool?)reader["UseSSL"] : null
                            };
                        }
                    }
                }

                if (company == null)
                {
                    throw new Exception("Company profile not found in database");
                }

                // Validate company email settings
                if (string.IsNullOrWhiteSpace(company.EmailServerHost))
                {
                    throw new Exception("Email server host is not configured");
                }

                if (string.IsNullOrWhiteSpace(company.EmailUsername))
                {
                    throw new Exception("Email username is not configured");
                }

                if (string.IsNullOrWhiteSpace(company.EmailPassword))
                {
                    throw new Exception("Email password is not configured");
                }

                if (string.IsNullOrWhiteSpace(company.CompanyEmail))
                {
                    throw new Exception("Company email is not configured");
                }

                int smtpPort = company.EmailServerPort ?? 587;
                bool useSSL = company.UseSSL ?? true;

                using var smtp = new System.Net.Mail.SmtpClient
                {
                    Host = company.EmailServerHost,
                    Port = smtpPort,
                    EnableSsl = useSSL,
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(
                        company.EmailUsername,
                        company.EmailPassword
                    ),
                    Timeout = 30000
                };

                var mail = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(
                        company.CompanyEmail,
                        company.CompanyName
                    ),
                    Subject = "Your account has been created",
                    Body = $@"Hello {fullName},

Your account has been created successfully.

Temporary password: {generatedPassword}

Please log in and change it immediately.

Regards,
{company.CompanyName}",
                    IsBodyHtml = false
                };

                mail.To.Add(email);

                await smtp.SendMailAsync(mail);

                emailSent = true;
            }
            catch (Exception emailEx)
            {
                emailError = emailEx.Message;
                Console.WriteLine($"[EMAIL FAILURE] {emailError}");
            }

            return Ok(new
            {
                success = true,
                message = "User added successfully.",
                userId = newUserId,
                emailSent = emailSent,
                emailError = emailError
            });
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { success = false, message = "Email is required." });

            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return Ok(new { success = true, message = "If an account exists, a reset link has been sent." });

            // Generate secure token
            string rawToken = TokenHelper.GenerateSecureToken();
            string tokenHash = TokenHelper.HashToken(rawToken);

            // ✅ CRITICAL LOGGING
            Console.WriteLine("=================================================");
            Console.WriteLine($"[FORGOT PASSWORD] Email: {request.Email}");
            Console.WriteLine($"[FORGOT PASSWORD] Raw Token: {rawToken}");
            Console.WriteLine($"[FORGOT PASSWORD] Token Hash: {tokenHash}");
            Console.WriteLine($"[FORGOT PASSWORD] Raw Token Length: {rawToken.Length}");
            Console.WriteLine("=================================================");

            var resetToken = new PasswordResetToken
            {
                UserId = user.UserID,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<PasswordResetToken>().Add(resetToken);
            await _context.SaveChangesAsync();

            // Get company email config
            var companyList = await _context.Set<PMCompanyProfileDto>()
                .FromSqlRaw(@"SELECT TOP 1 
                CompanyName, 
                CompanyEmail, 
                Motto, 
                CompanyPhone,
                PhysicalAddress,
                PostalAddress,
                EmailServerHost, 
                EmailServerPort, 
                EmailUsername, 
                EmailPassword,
                UseSSL,
                ProfilePic
              FROM ProjectManagement.CompanyProfile")
                .ToListAsync();

            var company = companyList.FirstOrDefault();

            if (company == null)
                return StatusCode(500, new { success = false, message = "Company email config not found." });

            string resetLink = $"http://localhost:3000/reset-password?token={rawToken}";

            Console.WriteLine($"[FORGOT PASSWORD] Reset Link: {resetLink}");

            try
            {
                using var smtpClient = new System.Net.Mail.SmtpClient(company.EmailServerHost)
                {
                    Port = company.EmailServerPort ?? 587,
                    Credentials = new System.Net.NetworkCredential(company.EmailUsername, company.EmailPassword),
                    EnableSsl = company.UseSSL ?? true,
                };

                var mail = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(company.CompanyEmail, company.CompanyName),
                    Subject = "Reset your password",
                    Body = $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #333;'>{company.CompanyName}</h2>
                <p style='color: #666; font-style: italic;'>{company.Motto}</p>
                <hr style='border: 1px solid #eee;'/>
                <p>Hello {user.FullName},</p>
                <p>You requested to reset your password.</p>
                <p>Please click the link below to reset your password (expires in 30 minutes):</p>
                <p><a href='{resetLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Reset Password</a></p>
                <p>Or copy and paste this link into your browser:</p>
                <p style='word-break: break-all; color: #666;'>{resetLink}</p>
                <p><strong>DEBUG - Raw Token:</strong> {rawToken}</p>
                <p>If you did not request this, please ignore this email.</p>
                <p>Best regards,<br/>{company.CompanyName} Team</p>
              </div>",
                    IsBodyHtml = true
                };

                mail.To.Add(user.Email);

                await smtpClient.SendMailAsync(mail);
                Console.WriteLine("[FORGOT PASSWORD] Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL FAILURE] {ex.Message}");
                return StatusCode(500, new { success = false, message = "Failed to send reset email." });
            }

            return Ok(new { success = true, message = "If an account exists, a reset link has been sent." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new { success = false, message = "Token and new password are required." });

            Console.WriteLine("=================================================");
            Console.WriteLine($"[RESET PASSWORD] Received Token: {request.Token}");
            Console.WriteLine($"[RESET PASSWORD] Token Length: {request.Token.Length}");

            string tokenHash = TokenHelper.HashToken(request.Token);
            Console.WriteLine($"[RESET PASSWORD] Computed Hash: {tokenHash}");

            // Debug - show all tokens
            var allTokens = await _context.PasswordResetTokens
                .Where(t => !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            Console.WriteLine($"[RESET PASSWORD] Valid tokens in DB: {allTokens.Count}");
            foreach (var t in allTokens)
            {
                Console.WriteLine($"  - Hash: {t.TokenHash}, UserId: {t.UserId}, Expires: {t.ExpiresAt}");
                Console.WriteLine($"  - Match: {t.TokenHash == tokenHash}");
            }
            Console.WriteLine("=================================================");

            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t =>
                    t.TokenHash == tokenHash &&
                    !t.IsUsed &&
                    t.ExpiresAt > DateTime.UtcNow);

            if (resetToken == null)
            {
                Console.WriteLine("[RESET PASSWORD] ❌ Token NOT found!");
                return BadRequest(new { success = false, message = "Invalid or expired token." });
            }

            Console.WriteLine($"[RESET PASSWORD] ✅ Token found! UserId: {resetToken.UserId}");

            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.UserID == resetToken.UserId);

            if (user == null)
                return BadRequest(new { success = false, message = "User not found." });

            user.passwordHash = HashHelper.ComputeSha256HashBytes(request.NewPassword);
            resetToken.IsUsed = true;

            await _context.SaveChangesAsync();

            Console.WriteLine("[RESET PASSWORD] ✅ Password reset successfully!");
            return Ok(new { success = true, message = "Password has been reset successfully." });
        }
    }

 
    // ========================= DTOs =========================
    public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}

public class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; }

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; }
}
public class AddPMUserDto
    {
        public int? EmployeeID { get; set; }
        public int? ClientID { get; set; }
        public int RoleId { get; set; }
        public int CreatedBy { get; set; }
        public int? StatusId { get; set; }
    }

    public class UserEmailDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class EmployeeClientDto
    {
        public int EntityID { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? EntityType { get; set; }
        public int? UserID { get; set; }
        public string? RoleName { get; set; }
    }

    public class GetAllRolesDto
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string? Description { get; set; }
    }

    public class AddRoleDto
    {
        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;
        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateRoleDto
    {
        [Required]
        public int RoleID { get; set; }

        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class DeleteRoleDto
    {
        public int RoleID { get; set; }
    }

    public class ResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}


