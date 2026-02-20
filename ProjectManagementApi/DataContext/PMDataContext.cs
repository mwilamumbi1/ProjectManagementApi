using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.Controllers;
 
using ProjectManagementApi.dTOs;
using ProjectManagementApi.Dtos;
using ProjectManagementApi.DTos;
using ProjectManagementApi.DTos.ProjectManagementApi.DTos;
using ProjectManagementApi.DTOs;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.DataContext
{
    public class PMDataContext : DbContext
    {
        public PMDataContext(DbContextOptions<PMDataContext> options) : base(options)
        {
        }

        public DbSet<BillingSummaryDto> BillingSummaryDtos { get; set; } = null!;
        public DbSet<EmployeeWorkloadDto> EmployeeWorkloadDtos { get; set; } = null!;
        public DbSet<BudgetVsActualDto> BudgetVsActualDtos { get; set; } = null!;
        public DbSet<RevenueByClientDto> RevenueByClientDtos { get; set; } = null!;
        public DbSet<IssuesReportDto> IssuesReportDtos { get; set; } = null!;

        public DbSet<ProjectSummaryDto> ProjectSummaryDtos { get; set; } = null!;
        public DbSet<EmployeeTimesheetDto> EmployeeTimesheetDtos { get; set; } = null!;
        public DbSet<ClientProjectDto> ClientProjectDtoss { get; set; } = null!;
        public DbSet<ClientsProjectsDto> ClientsProjectsDtos { get; set; } = null!;
        public DbSet<apiResponseDto> ApiResponseDtos { get; set; }

        public DbSet<BillingDto> BillingDtos { get; set; }
        public DbSet<BillingStatusDto> BillingStatusDtos { get; set; }
        public DbSet<EnhancedApiResponse> EnhancedApiResponseDtos { get; set; }
        public DbSet<UpdateBillingResponse> UpdateBillingResponseDtos { get; set; }

        public DbSet<BudgetDto> BudgetDtos { get; set; }
        public DbSet<UpdateBudgetResponse> UpdateBudgetResponseDtos { get; set; }

        public DbSet<CostItemDto> CostItemDtos { get; set; }
        public DbSet<CostItemByBudgetDto> CostItemByBudgetDtos { get; set; }
        public DbSet<UpdateCostItemResponse> UpdateCostItemResponseDtos { get; set; }

        // ✅ Clients
        public DbSet<ClientDto> ClientDtos { get; set; }
        public DbSet<ClientProjectDto> ClientProjectDtos { get; set; }

        // ✅ Permissions / Roles
        public DbSet<GetPermissionDto> GetPermissionDtos { get; set; }
        public DbSet<PermissionDto> PermissionDtos { get; set; }
        public DbSet<RolePermissionDetailDto> RolePermissionDetailDtos { get; set; }
        public DbSet<GetAuditLogDto> GetAuditLogDtos { get; set; }

        // ✅ Tasks
        public DbSet<GetAllTasksResult> GetAllTasks { get; set; }
        public DbSet<GetAllTaskAssignmentsResult> GetAllTaskAssignments { get; set; }
        public DbSet<GetAllTaskStatusResult> GetAllTaskStatus { get; set; }

        public DbSet<TimeEntryResultDto> TimeEntryResults { get; set; }
        public DbSet<DbResponse> DBResponse { get; set; }
 
        // ✅ Generic SP Responses
        public DbSet<DbResponse> DbResponse { get; set; }



        public DbSet<PMCompanyProfileDto> PMCompanyProfileDtos { get; set; }
        public DbSet<PortfolioDto> PortfolioDtos { get; set; }
        public DbSet<PortfolioProjectDto> PortfolioProjectDtos { get; set; }

        // Generic SP Responses
        public DbSet<GenericResponse> GenericResponse { get; set; }
        public DbSet<RowAffectedDto> RowsAffectedDtos { get; set; }
        // ✅ Resource Allocation SP responses
        public DbSet<SPResult> SPResults { get; set; }
        public DbSet<EmployeeWorkloadResult> EmployeeWorkloadResults { get; set; }
        public DbSet<ResourceAllocationDto> ResourceAllocations { get; set; }


        public DbSet<TaskUpdateResponse> TaskUpdateResponses { get; set; }
        public DbSet<EmployeesDto> EmployeesDtos { get; set; }
        public DbSet<IssueDto> IssueDtos { get; set; }
        public DbSet<SPResult> SPResult { get; set; }
        public DbSet<DeleteBudgetResult> DeleteBudgetResults { get; set; }
        public DbSet<StoredProcedureResponse> StoredProcedureResponse { get; set; }
        public DbSet<IssueResolutionDto> IssueResolutionDtos { get; set; }
        public DbSet<InactiveUserDto> InactiveUserDto { get; set; }

        public DbSet<ClientIssueDto> ClientIssueDtos { get; set; } = null!;

        // ====================================
        // EMPLOYEE DETAILS - DASHBOARD DbSets
        // ====================================
        public DbSet<EmployeeSummaryDto> EmployeeSummaryDtos { get; set; }
        public DbSet<RecentActivityDto> RecentActivityDtos { get; set; }
        public DbSet<HoursWorkedChartDto> HoursWorkedChartDtos { get; set; }
     

        public DbSet<StatusDistributionDto> StatusDistributionDtos { get; set; }
        public DbSet<ProjectStatusDistributionDto> ProjectStatusDistributionDtos { get; set; }

        // ====================================
        // EMPLOYEE DETAILS - MY PROJECTS DbSets
        // ====================================
        public DbSet<MyProjectDto> MyProjectDtos { get; set; }

        // ====================================
        // EMPLOYEE DETAILS - MY TASKS DbSets
        // ====================================
        public DbSet<MyTaskDto> MyTaskDtos { get; set; }
        public DbSet<ClientProjectByIDDto> ClientProjectByIDDto { get; set; }
        // ====================================
        // EMPLOYEE DETAILS - MY ISSUES DbSets
        // ====================================
        public DbSet<MyIssueDto> MyIssueDtos { get; set; }

        // ====================================
        // EMPLOYEE DETAILS - TIME ENTRIES DbSets
        // ====================================
        public DbSet<TaskTimeEntryDto> TaskTimeEntryDtos { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }


        // ====================================
        // EMPLOYEE DETAILS - FILTER OPTIONS DbSets
        // ====================================
        public DbSet<ProjectFilterDto> ProjectFilterDtos { get; set; }
        public DbSet<ClientFilterDto> ClientFilterDtos { get; set; }
        public DbSet<TaskStatusFilterDto> TaskStatusFilterDtos { get; set; }
        public DbSet<ProjectStatusFilterDto> ProjectStatusFilterDtos { get; set; }
        public DbSet<BillingStatusFilterDto> BillingStatusFilterDtos { get; set; }
        public DbSet<PriorityFilterDto> PriorityFilterDtos { get; set; }
        public DbSet<EmployeeClientDto> EmployeeClientDto { get; set; }
        public DbSet<GetAllRolesDto> GetAllRolesDto { get; set; }

        // Counts results (from CountsController)

        public DbSet<MilestoneDto> MilestoneDtos { get; set; }
        public DbSet<SPResult> SPResultss { get; set; }
        public object EmployeeWorkloadDtoss { get; internal set; }
        public IEnumerable<object> Users { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<BillingSummaryDto>().HasNoKey().ToView(null);
            modelBuilder.Entity<EmployeeWorkloadDto>().HasNoKey().ToView(null);
            modelBuilder.Entity<BudgetVsActualDto>().HasNoKey().ToView(null);
            modelBuilder.Entity<RevenueByClientDto>().HasNoKey().ToView(null);
            modelBuilder.Entity<IssuesReportDto>().HasNoKey().ToView(null);

            modelBuilder.Entity<ProjectSummaryDto>().HasNoKey().ToView(null);
            modelBuilder.Entity<EmployeeTimesheetDto>().HasNoKey().ToView(null);
            modelBuilder.Entity<ClientsProjectsDto>().HasNoKey().ToView(null);
            modelBuilder.Entity<PMCompanyProfileDto>(entity =>{entity.HasNoKey();entity.ToView(null);});
   
            modelBuilder.Entity<LoginDto>().HasNoKey();
            modelBuilder.Entity<GetPermissionDto>().HasNoKey();
            modelBuilder.Entity<PermissionDto>().HasNoKey();
            modelBuilder.Entity<RolePermissionDetailDto>().HasNoKey();
            modelBuilder.Entity<GetAuditLogDto>().HasNoKey();
            modelBuilder.Entity<InactiveUserDto>().HasNoKey().ToView(null);
            modelBuilder.Entity<TaskUpdateResponse>().HasNoKey();
            modelBuilder.Entity<ClientDto>().HasNoKey();
            modelBuilder.Entity<ClientProjectDto>().HasNoKey();
            modelBuilder.Entity<EmployeeClientDto>().HasNoKey();
            modelBuilder.Entity<GetAllRolesDto>().HasNoKey();
            modelBuilder.Entity<EmployeesDto>().HasNoKey();
            modelBuilder.Entity<IssueResolutionDto>().HasNoKey().ToView(null);
            modelBuilder.Entity<DeleteUserResultDto>().HasNoKey();
            modelBuilder.Entity<apiResponseDto>().HasNoKey();
            modelBuilder.Entity<DeleteUserResultDto>().HasNoKey();
            modelBuilder.Entity<BillingDto>().HasNoKey();
            modelBuilder.Entity<BillingStatusDto>().HasNoKey();
            modelBuilder.Entity<EnhancedApiResponse>().HasNoKey();
            modelBuilder.Entity<UpdateBillingResponse>().HasNoKey();

            modelBuilder.Entity<BudgetDto>().HasNoKey();
            modelBuilder.Entity<UpdateBudgetResponse>().HasNoKey();
            //modelBuilder.Entity<CountResult>().HasNoKey();
            modelBuilder.Entity<CostItemDto>().HasNoKey();
            modelBuilder.Entity<CostItemByBudgetDto>().HasNoKey();
            modelBuilder.Entity<UpdateCostItemResponse>().HasNoKey();

            modelBuilder.Entity<ClientProjectByIDDto>()
           .HasNoKey()
           .ToView(null);

            modelBuilder.Entity<IssueResolutionDto>().HasNoKey();
            modelBuilder.Entity<EmployeeDto>(entity =>{entity.HasNoKey();entity.ToView(null);});

            modelBuilder.Entity<GenericResponse>().HasNoKey();
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<GetAllTasksResult>().HasNoKey();
            modelBuilder.Entity<GetAllTaskAssignmentsResult>().HasNoKey();
            modelBuilder.Entity<GetAllTaskStatusResult>().HasNoKey();

            modelBuilder.Entity<PortfolioDto>().HasNoKey();
            modelBuilder.Entity<PortfolioProjectDto>().HasNoKey();
            modelBuilder.Entity<GenericResponse>().HasNoKey();
            modelBuilder.Entity<RowAffectedDto>().HasNoKey();
            modelBuilder.Entity<UserEmailDto>().HasNoKey().ToView(null);


            modelBuilder.Entity<DbResponse>().HasNoKey();
        

            modelBuilder.Entity<TimeEntryResultDto>().HasNoKey();
            modelBuilder.Entity<DbResponse>().HasNoKey();

            modelBuilder.Entity<SPResult>().HasNoKey();
            modelBuilder.Entity<EmployeeWorkloadResult>().HasNoKey();
            modelBuilder.Entity<ResourceAllocationDto>().HasNoKey(); // For GetAllResourceAllocations

            modelBuilder.Entity<IssueDto>().HasNoKey();
            modelBuilder.Entity<SPResult>().HasNoKey();
 

            modelBuilder.Entity<MilestoneDto>().HasNoKey();
            modelBuilder.Entity<SPResultss>().HasNoKey();
            modelBuilder.Entity<DeleteBudgetResult>().HasNoKey();


            modelBuilder.Entity<EmployeeSummaryDto>().HasNoKey();
            modelBuilder.Entity<RecentActivityDto>().HasNoKey();
            modelBuilder.Entity<HoursWorkedChartDto>().HasNoKey();
            modelBuilder.Entity<StatusDistributionDto>().HasNoKey();
            modelBuilder.Entity<ProjectStatusDistributionDto>().HasNoKey();
            modelBuilder.Entity<ClientProjectByIDDto>().HasNoKey().ToView(null);

            // ====================================
            // EMPLOYEE DETAILS - MY PROJECTS - HasNoKey
            // ====================================
            modelBuilder.Entity<MyProjectDto>().HasNoKey();

            // ====================================
            // EMPLOYEE DETAILS - MY TASKS - HasNoKey
            // ====================================
            modelBuilder.Entity<MyTaskDto>().HasNoKey();

            // ====================================
            // EMPLOYEE DETAILS - MY ISSUES - HasNoKey
            // ====================================
            modelBuilder.Entity<MyIssueDto>().HasNoKey();

            // ====================================
            // EMPLOYEE DETAILS - TIME ENTRIES - HasNoKey
            // ====================================
            modelBuilder.Entity<TaskTimeEntryDto>().HasNoKey();
            modelBuilder.Entity<ClientDto>().HasNoKey();
            modelBuilder.Entity<ClientProjectDto>().HasNoKey();
            modelBuilder.Entity<ClientIssueDto>().HasNoKey();
        
        // ====================================
        // EMPLOYEE DETAILS - FILTER OPTIONS - HasNoKey
        // ====================================
        modelBuilder.Entity<ProjectFilterDto>().HasNoKey();
            modelBuilder.Entity<ClientFilterDto>().HasNoKey();
            modelBuilder.Entity<TaskStatusFilterDto>().HasNoKey();
            modelBuilder.Entity<ProjectStatusFilterDto>().HasNoKey();
            modelBuilder.Entity<BillingStatusFilterDto>().HasNoKey();
            modelBuilder.Entity<PriorityFilterDto>().HasNoKey();




            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users", "core");
                entity.HasKey(e => e.UserID);
                entity.Property(e => e.UserID).ValueGeneratedOnAdd();
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.passwordHash).HasColumnType("varbinary(64)");
                entity.Property(e => e.CreatedBy).HasMaxLength(50);
            });

            // Configure PasswordResetToken entity
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.ToTable("PasswordResetTokens", "core"); // Adjust table name/schema as needed
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId);
            });

        }
    }
}
