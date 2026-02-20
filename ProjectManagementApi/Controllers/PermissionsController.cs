namespace ProjectManagementApi.Dtos
{
    public class UpdateRolePermissionDto
    {
        public int RolePermissionID { get; set; }
        public int RoleID { get; set; }
        public int PermissionID { get; set; }
        public int GrantedBy { get; set; }
    }

    public class GetPermissionDto
    {
        public int PermissionID { get; set; }
        public string? PermissionName { get; set; }
        public string? Description { get; set; }
        public string? Module { get; set; }
    }

    public class PermissionDto
    {
        public int UserID { get; set; }
        public string? PermissionNames { get; set; }
    }

    public class RolePermissionDto
    {
        public int RoleID { get; set; }
        public int PermissionID { get; set; }
        public int GrantedBy { get; set; }
    }

    public class RolePermissionDetailDto
    {
        public int RolePermissionID { get; set; }
        public string? RoleName { get; set; }
        public string? PermissionName { get; set; }
        public DateTime GrantedAt { get; set; }
        public string? GrantedByUserName { get; set; }
    }

    public class ResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class DeleteRolePermissionDto
    {
        public int RolePermissionID { get; set; }
    }
}