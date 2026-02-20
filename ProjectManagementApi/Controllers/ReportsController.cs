using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.DataContext;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Data;
using System.Text;
using Document = QuestPDF.Fluent.Document;

namespace ProjectManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly PMDataContext _context;

        public ReportsController(PMDataContext context)
        {
            _context = context;
        }

        // Helper method to fetch company profile using ADO.NET for reliability
        private async Task<CompanyProfileDto?> GetCompanyProfile()
        {
            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "ProjectManagement.GetCompanyProfile";
                command.CommandType = CommandType.StoredProcedure;

                await _context.Database.OpenConnectionAsync();

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var profile = new CompanyProfileDto
                    {
                        Success = reader.GetBoolean(reader.GetOrdinal("Success")),
                        Message = reader.IsDBNull(reader.GetOrdinal("Message")) ? null : reader.GetString(reader.GetOrdinal("Message")),
                        CompanyName = reader.IsDBNull(reader.GetOrdinal("CompanyName")) ? null : reader.GetString(reader.GetOrdinal("CompanyName")),
                        CompanyEmail = reader.IsDBNull(reader.GetOrdinal("CompanyEmail")) ? null : reader.GetString(reader.GetOrdinal("CompanyEmail")),
                        Motto = reader.IsDBNull(reader.GetOrdinal("Motto")) ? null : reader.GetString(reader.GetOrdinal("Motto")),
                        CompanyPhone = reader.IsDBNull(reader.GetOrdinal("CompanyPhone")) ? null : reader.GetString(reader.GetOrdinal("CompanyPhone")),
                        PhysicalAddress = reader.IsDBNull(reader.GetOrdinal("PhysicalAddress")) ? null : reader.GetString(reader.GetOrdinal("PhysicalAddress")),
                        PostalAddress = reader.IsDBNull(reader.GetOrdinal("PostalAddress")) ? null : reader.GetString(reader.GetOrdinal("PostalAddress")),
                        EmailServerHost = reader.IsDBNull(reader.GetOrdinal("EmailServerHost")) ? null : reader.GetString(reader.GetOrdinal("EmailServerHost")),
                        EmailServerPort = reader.IsDBNull(reader.GetOrdinal("EmailServerPort")) ? null : reader.GetInt32(reader.GetOrdinal("EmailServerPort")),
                        EmailUsername = reader.IsDBNull(reader.GetOrdinal("EmailUsername")) ? null : reader.GetString(reader.GetOrdinal("EmailUsername")),
                        UseSSL = reader.IsDBNull(reader.GetOrdinal("UseSSL")) ? null : reader.GetBoolean(reader.GetOrdinal("UseSSL"))
                    };

                    return profile.Success ? profile : null;
                }
                return null;
            }
            catch
            {
                return null;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        // Helper method to fetch data and handle immediate export
        private async Task<IActionResult> GetReportData<T>(string storedProcedureName, string fileName, string format) where T : class
        {
            // Use the DbSet corresponding to the DTO type T
            var dbSet = _context.Set<T>();

            // Execute the raw SQL stored procedure
            var result = await dbSet
                .FromSqlRaw($"EXEC ProjectManagement.{storedProcedureName}")
                .ToListAsync();

            return await Export(result, format, fileName);
        }

        // ========================= REPORT ENDPOINTS ==============================
        // All endpoints now use the simplified GetReportData helper method

        [HttpGet("GetAllBillingSummary")]
        public async Task<IActionResult> GetBillingSummary([FromQuery] string format = "json")
        {
            return await GetReportData<BillingSummaryDto>("GetBillingReport", "BillingReport", format);
        }

        [HttpGet("GetAllBudgetVsActual")]
        public async Task<IActionResult> GetBudgetVsActual([FromQuery] string format = "json")
        {
            return await GetReportData<BudgetVsActualDto>("GetBudgetVsActualReport", "BudgetVsActualReport", format);
        }

        [HttpGet("GetAllIssues")]
        public async Task<IActionResult> GetIssuesReport([FromQuery] string format = "json")
        {
            return await GetReportData<IssuesReportDto>("GetIssueTrackingReport", "IssueTrackingReport", format);
        }

        [HttpGet("GetAllProjectSummary")]
        public async Task<IActionResult> GetProjectSummary([FromQuery] string format = "json")
        {
            return await GetReportData<ProjectSummaryDto>("GetProjectSummaryReport", "ProjectSummaryReport", format);
        }

        [HttpGet("GetAllEmployeeTimesheets")]
        public async Task<IActionResult> GetEmployeeTimesheet([FromQuery] string format = "json")
        {
            return await GetReportData<EmployeeTimesheetDto>("GetEmployeeTimesheetReport", "EmployeeTimesheetReport", format);
        }

        [HttpGet("GetAllClientProjectAssignments")]
        public async Task<IActionResult> GetClientProjectAssignments([FromQuery] string format = "json")
        {
            return await GetReportData<ClientsProjectsDto>("GetClientProjectReport", "ClientProjectReport", format);
        }

        [HttpGet("GetAllEmployeeWorkload")]
        public async Task<IActionResult> GetEmployeeWorkload([FromQuery] string format = "json")
        {
            return await GetReportData<EmployeeWorkloadDto>("GetEmployeeWorkloadReport", "EmployeeWorkloadReport", format);
        }

        [HttpGet("GetAllRevenueByClient")]
        public async Task<IActionResult> GetRevenueByClient([FromQuery] string format = "json")
        {
            return await GetReportData<RevenueByClientDto>("GetRevenueByClientReport", "RevenueByClientReport", format);
        }

        // ========================= EXPORT HANDLER ==============================
        private async Task<IActionResult> Export<T>(List<T> data, string format, string fileName)
        {
            if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
                return await ExportToCsv(data, fileName);

            if (format.Equals("excel", StringComparison.OrdinalIgnoreCase))
                return await ExportToExcel(data, fileName);

            if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
                return await ExportToPdf(data, fileName);

            // Default JSON
            return Ok(data);
        }

        // ------------------------ CSV Export ------------------------
        private async Task<IActionResult> ExportToCsv<T>(List<T> data, string fileName)
        {
            var companyProfile = await GetCompanyProfile();
            var props = typeof(T).GetProperties();
            var sb = new StringBuilder();

            // Add Company information to CSV header
            sb.AppendLine($"Company: {companyProfile?.CompanyName ?? "NECOR PSL Software"}");
            if (!string.IsNullOrWhiteSpace(companyProfile?.Motto))
            {
                sb.AppendLine($"Motto: {companyProfile.Motto}");
            }
            if (!string.IsNullOrWhiteSpace(companyProfile?.CompanyEmail))
            {
                sb.AppendLine($"Email: {companyProfile.CompanyEmail}");
            }
            sb.AppendLine($"Report: {fileName}");
            sb.AppendLine($"Created On: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("");

            // Data Header Row
            sb.AppendLine(string.Join(",", props.Select(p => p.Name)));

            // Data Rows
            foreach (var row in data)
            {
                sb.AppendLine(string.Join(",", props.Select(p => p.GetValue(row, null)?.ToString()?.Replace(",", "") ?? "")));
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"{fileName}.csv");
        }

        // ------------------------ Excel Export (Beautified with ClosedXML) ------------------------
        private async Task<IActionResult> ExportToExcel<T>(List<T> data, string fileName)
        {
            var companyProfile = await GetCompanyProfile();
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Report");

            // --- Header / Metadata ---
            int currentRow = 1;

            // Company Name
            worksheet.Cell(currentRow, 1).Value = companyProfile?.CompanyName ?? "NECOR PSL Software";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 14;
            currentRow++;

            // Company Motto
            if (!string.IsNullOrWhiteSpace(companyProfile?.Motto))
            {
                worksheet.Cell(currentRow, 1).Value = companyProfile.Motto;
                worksheet.Cell(currentRow, 1).Style.Font.Italic = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 10;
                currentRow++;
            }

            // Company Email
            if (!string.IsNullOrWhiteSpace(companyProfile?.CompanyEmail))
            {
                worksheet.Cell(currentRow, 1).Value = $"Email: {companyProfile.CompanyEmail}";
                currentRow++;
            }

            currentRow++; // Blank line

            // Report Title
            worksheet.Cell(currentRow, 1).Value = $"{fileName} Report";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            currentRow++;

            worksheet.Cell(currentRow, 1).Value = $"Created On: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            currentRow++;
            currentRow++; // Blank line before data table

            // --- Insert Data Table ---
            if (!data.Any())
            {
                int col = 1;
                foreach (var prop in typeof(T).GetProperties())
                {
                    worksheet.Cell(currentRow, col++).Value = prop.Name;
                }
                worksheet.Cell(currentRow + 1, 1).Value = "No records found.";
                worksheet.ColumnsUsed().AdjustToContents();
            }
            else
            {
                var dataRange = worksheet.Cell(currentRow, 1).InsertTable(data);

                // --- Beautification ---
                var headerRow = dataRange.FirstRow();
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                worksheet.ColumnsUsed().AdjustToContents();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{fileName}.xlsx");
        }

        // ------------------------ PDF Export (Beautified with QuestPDF) ------------------------
        private async Task<IActionResult> ExportToPdf<T>(List<T> data, string fileName)
        {
            var companyProfile = await GetCompanyProfile();
            var props = typeof(T).GetProperties();

            var bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    // --- HEADER ---
                    page.Header().PaddingBottom(15).Column(headerCol =>
                    {
                        // Company Name
                        headerCol.Item().AlignCenter().Text(companyProfile?.CompanyName ?? "NECOR PSL Software")
                            .Bold().FontSize(16).FontColor(Colors.Blue.Darken2);

                        // Company Motto
                        if (!string.IsNullOrWhiteSpace(companyProfile?.Motto))
                        {
                            headerCol.Item().AlignCenter().Text(companyProfile.Motto)
                                .Italic().FontSize(10).FontColor(Colors.Grey.Darken1);
                        }

                        // Company Email
                        if (!string.IsNullOrWhiteSpace(companyProfile?.CompanyEmail))
                        {
                            headerCol.Item().AlignCenter().Text(companyProfile.CompanyEmail)
                                .FontSize(9).FontColor(Colors.Grey.Medium);
                        }

                        // Separator Line
                        headerCol.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                        // Report Title
                        headerCol.Item()
                            .PaddingTop(10)
                            .PaddingBottom(5)
                            .Text($"{fileName}").Bold().FontSize(18).AlignCenter();

                        // Creation Date
                        headerCol.Item().AlignRight().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                            .FontSize(8).FontColor(Colors.Grey.Medium);
                    });

                    // --- CONTENT (Table) ---
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var _ in props)
                                columns.RelativeColumn();
                        });

                        // Header Cells
                        table.Header(header =>
                        {
                            foreach (var prop in props)
                            {
                                header.Cell()
                                    .Background(Colors.BlueGrey.Darken1)
                                    .Padding(5)
                                    .Text(prop.Name).Bold().FontColor(Colors.White).FontSize(10);
                            }
                        });

                        // Data Rows
                        foreach (var item in data)
                        {
                            var isEvenRow = data.IndexOf(item) % 2 == 0;
                            var cellBackground = isEvenRow ? Colors.White : Colors.Grey.Lighten4;

                            foreach (var prop in props)
                            {
                                var val = prop.GetValue(item, null)?.ToString() ?? "";

                                table.Cell()
                                    .Background(cellBackground)
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .Padding(4)
                                    .Text(val).FontSize(9);
                            }
                        }
                    });

                    // --- FOOTER ---
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ").FontSize(8);
                            x.CurrentPageNumber().FontSize(8);
                            x.Span(" of ").FontSize(8);
                            x.TotalPages().FontSize(8);
                        });
                });
            }).GeneratePdf();

            return File(bytes, "application/pdf", $"{fileName}.pdf");
        }
    }

    // ========================= DTOs ==============================

    public class CompanyProfileDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyEmail { get; set; }
        public string? Motto { get; set; }
        public string? CompanyPhone { get; set; }
        public string? PhysicalAddress { get; set; }
        public string? PostalAddress { get; set; }
        public string? EmailServerHost { get; set; }
        public int? EmailServerPort { get; set; }
        public string? EmailUsername { get; set; }
        public bool? UseSSL { get; set; }
    }

    public class BillingSummaryDto
    {
        public string? InvoiceNumber { get; set; }
        public string? ProjectName { get; set; }
        public string? BillingName { get; set; }
        public decimal? Amount { get; set; }
        public string? BillingStatus { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class EmployeeWorkloadDto
    {
        public string? EmployeeName { get; set; }
        public int? NumberOfProjects { get; set; }
        public int? TasksAssigned { get; set; }
        public decimal? TotalHoursWorked { get; set; }
    }

    public class BudgetVsActualDto
    {
        public string? ProjectName { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public decimal? Variance { get; set; }
        public string? BudgetStatus { get; set; }
    }

    public class RevenueByClientDto
    {
        public string? ClientName { get; set; }
        public decimal? TotalBilling { get; set; }
        public string? Period { get; set; }
    }

    public class IssuesReportDto
    {
        public int? IssueID { get; set; }
        public string? ProjectName { get; set; }
        public string? IssueTitle { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? ResolvedBy { get; set; }
    }

    public class ProjectSummaryDto
    {
        public int? ProjectID { get; set; }
        public string? ProjectName { get; set; }
        public string? ProjectStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ClientName { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public decimal? TotalBilled { get; set; }
        public int? TotalTasks { get; set; }
    }

    public class EmployeeTimesheetDto
    {
        public string? FullName { get; set; }
        public string? ProjectName { get; set; }
        public string? TaskName { get; set; }
        public DateTime? DateWorked { get; set; }
        public decimal? HoursWorked { get; set; }
        public string? Notes { get; set; }
    }

    public class ClientsProjectsDto
    {
        public string? ClientName { get; set; }
        public string? ProjectName { get; set; }
        public string? AssignedEmployee { get; set; }
        public DateTime? AssignedDate { get; set; }
    }
}