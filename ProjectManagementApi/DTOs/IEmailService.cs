// Core.HumanResourceManagementApi.DTOs/IEmailService.cs
using System.Threading.Tasks;

namespace Core.HumanResourceManagementApi.DTOs
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent);
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    }
}