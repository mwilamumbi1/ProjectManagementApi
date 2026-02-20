using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 

namespace ProjectManagementApi.Models
{
 
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string TokenHash { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public User User { get; set; }
    }
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime? CreatedAt { get; set; }
        public byte[] passwordHash { get; set; }  // varbinary(64) = byte[]
        public DateTime? PasswordExpiryDate { get; set; }
        public int? complexityId { get; set; }
        public int? RoleId { get; set; }
        public int? StatusId { get; set; }
        public string CreatedBy { get; set; }
    }

}
