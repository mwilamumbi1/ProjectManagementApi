using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApi.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string MessageType { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    // Project DTOs
    public class CreateProjectDto
    {
        [Required]
        [StringLength(200)]
        public string ProjectName { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
 

        public string UserID { get; set; } = "SYSTEM";
    }

    public class UpdateProjectDto
    {
        [Required]
        public int ProjectID { get; set; }

        [StringLength(200)]
        public string ProjectName { get; set; }

        public string Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? StatusID { get; set; }

        public string UserID { get; set; } = "SYSTEM";
    }

    public class ProjectDto
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? StatusID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }


    public class ProjectDependenciesDto
    {
        // Main project info
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }

        // Direct dependency counts
        public int TaskCount { get; set; }
        public int BudgetCount { get; set; }
        public int BillingCount { get; set; }
        public int ResourceAllocationCount { get; set; }
        public int PortfolioAssignmentCount { get; set; }

        // Indirect dependency counts
        public int TimeEntryCount { get; set; }
        public int TaskAssignmentCount { get; set; }
        public int CostItemCount { get; set; }

        // Deletion recommendation
        public string DeletionRecommendation { get; set; }

        // Lists of dependency details
        public List<TaskInfo> Tasks { get; set; } = new();
        public List<BillingInfo> Billings { get; set; } = new();
        public List<PortfolioInfo> Portfolios { get; set; } = new();
    }

    public class TaskInfo
    {
        public int TaskID { get; set; }
        public string TaskName { get; set; }
    }

    public class BillingInfo
    {
        public int BillingID { get; set; }
        public string BillingName { get; set; }
    }

    public class PortfolioInfo
    {
        public int PortfolioID { get; set; }
        public string PortfolioName { get; set; }
    }

    public class ProjectSummaryDetailsDto
    {
        // API Response fields
        public bool Success { get; set; }
        public string Message { get; set; }

        // Project Basic Info
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ProjectStatus { get; set; }

        // Budget Information
        public int? BudgetID { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public decimal? Variance { get; set; }
        public string BudgetStatus { get; set; }
        public DateTime? BudgetApprovedDate { get; set; }

        // Aggregated Counts
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalIssues { get; set; }
        public int OpenIssues { get; set; }
        public int TotalClients { get; set; }
        public int TotalResources { get; set; }
        public decimal TotalBilled { get; set; }

        // JSON String columns (from database)
        public string ClientsList { get; set; }
        public string TasksList { get; set; }
        public string IssuesList { get; set; }
        public string ResourcesList { get; set; }
        public string BillingsList { get; set; }

        // Deserialized Lists (populated after JSON parsing)
        public List<ClientSummaryDto> Clients { get; set; } = new();
        public List<TaskSummaryDto> Tasks { get; set; } = new();
        public List<IssueSummaryDto> Issues { get; set; } = new();
        public List<ResourceSummaryDto> Resources { get; set; } = new();
        public List<BillingSummaryItemDto> Billings { get; set; } = new();
    }

    public class ClientSummaryDto
    {
        public int ClientID { get; set; }
        public string ClientName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string AssignedEmployee { get; set; }
        public DateTime? AssignedDate { get; set; }
    }

    public class TaskSummaryDto
    {
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Priority { get; set; }
        public string TaskStatus { get; set; }
        public string AssignedEmployees { get; set; }
        public decimal TotalHoursWorked { get; set; }
    }

    public class IssueSummaryDto
    {
        public int IssueID { get; set; }
        public string IssueTitle { get; set; }
        public string IssueDescription { get; set; }
        public string IssueStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string ClientName { get; set; }
        public string ResolvedBy { get; set; }
        public string ResolutionNotes { get; set; }
        public DateTime? LastResolutionDate { get; set; }
    }

    public class ResourceSummaryDto
    {
        public int AllocationID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeEmail { get; set; }
        public string EmployeeRole { get; set; }
        public string ProjectRole { get; set; }
        public decimal AllocationPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public int TasksAssigned { get; set; }
    }

    public class BillingSummaryItemDto
    {
        public int BillingID { get; set; }
        public string InvoiceNumber { get; set; }
        public string BillingName { get; set; }
        public decimal Amount { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime DueDate { get; set; }
        public string BillingStatus { get; set; }
    }

}