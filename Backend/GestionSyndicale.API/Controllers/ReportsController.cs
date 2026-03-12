using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GestionSyndicale.Core.Interfaces;
using GestionSyndicale.Core.DTOs.Reports;
using GestionSyndicale.Infrastructure.Data;
using GestionSyndicale.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GestionSyndicale.API.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            ApplicationDbContext context,
            IPdfService pdfService,
            IConfiguration configuration,
            ILogger<ReportsController> logger)
        {
            _context = context;
            _pdfService = pdfService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("buildings/{buildingId}/payments-grid")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GenerateBuildingPaymentsReport(int buildingId, [FromBody] BuildingPaymentsReportRequest request)
        {
            try
            {
                // Vérifier que l'immeuble existe
                var building = await _context.Buildings
                    .Include(b => b.Residence)
                    .Include(b => b.Apartments)
                    .FirstOrDefaultAsync(b => b.Id == buildingId);

                if (building == null)
                {
                    return NotFound("Building not found");
                }

                // Valider les années
                if (request.Years == null || !request.Years.Any())
                {
                    return BadRequest("At least one year must be selected");
                }

                // Générer le PDF
                var pdfBytes = await _pdfService.GenerateBuildingPaymentsGridAsync(building, request.Years);

                // Nom du fichier
                var fileName = $"Etat_Paiements_{building.Name}_{DateTime.Now:yyyyMMdd}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating building payments report for building {BuildingId}", buildingId);
                return StatusCode(500, "An error occurred while generating the report");
            }
        }

        [HttpGet("financial-summary")]
        public async Task<ActionResult<FinancialSummaryDto>> GetFinancialSummary(
            [FromQuery] string from,
            [FromQuery] string to)
        {
            try
            {
                if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
                {
                    return BadRequest("Invalid date format. Use YYYY-MM-DD.");
                }

                if (fromDate > toDate)
                {
                    return BadRequest("From date must be before or equal to To date.");
                }

                var monthlyContributionAmount = _configuration.GetValue<decimal>("Finance:MonthlyContributionAmount", 100);

                // 1. Calculate Contributions (MonthlyPayments where IsPaid=true)
                var contributions = await _context.MonthlyPayments
                    .Where(mp => mp.IsPaid && mp.PaymentDate >= fromDate && mp.PaymentDate <= toDate)
                    .CountAsync() * monthlyContributionAmount;

                // 2. Calculate Other Revenues
                var otherRevenues = await _context.OtherRevenues
                    .Where(or => or.RevenueDate >= fromDate && or.RevenueDate <= toDate)
                    .SumAsync(or => or.Amount);

                // 3. Calculate Expenses
                var expenses = await _context.Expenses
                    .Where(e => e.ExpenseDate >= fromDate && e.ExpenseDate <= toDate)
                    .SumAsync(e => e.Amount);

                // 4. Monthly breakdown
                var monthlyData = new List<MonthlyFinancialDto>();
                var currentDate = new DateTime(fromDate.Year, fromDate.Month, 1);
                var endDate = new DateTime(toDate.Year, toDate.Month, 1);

                while (currentDate <= endDate)
                {
                    var monthStart = currentDate;
                    var monthEnd = currentDate.AddMonths(1).AddDays(-1);

                    var monthContributions = await _context.MonthlyPayments
                        .Where(mp => mp.IsPaid && mp.PaymentDate >= monthStart && mp.PaymentDate <= monthEnd)
                        .CountAsync() * monthlyContributionAmount;

                    var monthOtherRevenues = await _context.OtherRevenues
                        .Where(or => or.RevenueDate >= monthStart && or.RevenueDate <= monthEnd)
                        .SumAsync(or => or.Amount);

                    var monthExpenses = await _context.Expenses
                        .Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate <= monthEnd)
                        .SumAsync(e => e.Amount);

                    var monthTotal = monthContributions + monthOtherRevenues;
                    var monthResult = monthTotal - monthExpenses;

                    monthlyData.Add(new MonthlyFinancialDto
                    {
                        Month = currentDate.ToString("yyyy-MM"),
                        Contributions = monthContributions,
                        OtherRevenues = monthOtherRevenues,
                        TotalRevenues = monthTotal,
                        Expenses = monthExpenses,
                        NetResult = monthResult
                    });

                    currentDate = currentDate.AddMonths(1);
                }

                // 5. Expenses by category
                var expensesByCategory = await _context.Expenses
                    .Include(e => e.Supplier)
                    .Where(e => e.ExpenseDate >= fromDate && e.ExpenseDate <= toDate)
                    .Where(e => e.Supplier != null)
                    .GroupBy(e => e.Supplier.ServiceCategory)
                    .Select(g => new
                    {
                        CategoryId = 0,
                        CategoryName = g.Key,
                        Amount = g.Sum(e => e.Amount)
                    })
                    .ToListAsync();

                var expensesByCategoryDto = expensesByCategory.Select(e => new ExpenseByCategoryDto
                {
                    CategoryId = e.CategoryId,
                    CategoryName = e.CategoryName ?? "Sans catégorie",
                    Amount = e.Amount,
                    Percent = expenses > 0 ? (e.Amount / expenses * 100) : 0
                }).ToList();

                // 6. Other revenues by title
                var otherRevenuesByTitle = await _context.OtherRevenues
                    .Where(or => or.RevenueDate >= fromDate && or.RevenueDate <= toDate)
                    .GroupBy(or => or.Title)
                    .Select(g => new
                    {
                        Title = g.Key,
                        Amount = g.Sum(or => or.Amount)
                    })
                    .ToListAsync();

                var otherRevenuesByTitleDto = otherRevenuesByTitle.Select(r => new RevenueByTitleDto
                {
                    Title = r.Title,
                    Amount = r.Amount,
                    Percent = otherRevenues > 0 ? (r.Amount / otherRevenues * 100) : 0
                }).ToList();

                // 7. Collection rate calculation
                var apartmentsCount = await _context.Apartments.CountAsync();
                var monthsInPeriod = ((toDate.Year - fromDate.Year) * 12 + toDate.Month - fromDate.Month + 1);
                var expectedAmount = apartmentsCount * monthsInPeriod * monthlyContributionAmount;
                var collectedAmount = contributions;
                var collectionRate = expectedAmount > 0 ? (collectedAmount / expectedAmount * 100) : 0;

                var result = new FinancialSummaryDto
                {
                    From = fromDate.ToString("yyyy-MM-dd"),
                    To = toDate.ToString("yyyy-MM-dd"),
                    Totals = new FinancialTotalsDto
                    {
                        Contributions = contributions,
                        OtherRevenues = otherRevenues,
                        TotalRevenues = contributions + otherRevenues,
                        Expenses = expenses,
                        NetResult = (contributions + otherRevenues) - expenses
                    },
                    ByMonth = monthlyData,
                    ExpensesByCategory = expensesByCategoryDto,
                    OtherRevenuesByTitle = otherRevenuesByTitleDto,
                    CollectionRate = new CollectionRateDto
                    {
                        ExpectedAmount = expectedAmount,
                        CollectedAmount = collectedAmount,
                        Rate = collectionRate
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating financial summary");
                return StatusCode(500, "An error occurred while generating the financial summary");
            }
        }

        [HttpPost("financial-summary/pdf")]
        public async Task<IActionResult> GenerateFinancialSummaryPdf([FromBody] GeneratePdfRequest request)
        {
            try
            {
                // Get the financial summary data
                var summaryResult = await GetFinancialSummary(request.From, request.To);
                if (summaryResult.Result is not OkObjectResult okResult || okResult.Value is not FinancialSummaryDto summary)
                {
                    return BadRequest("Unable to generate financial summary");
                }

                // Get configuration
                var residenceName = _configuration["Syndic:ResidenceName"] ?? "Résidence";
                var legalName = _configuration["Syndic:LegalName"] ?? "Syndic";

                // Generate PDF
                var pdfService = new FinancialReportPdfService(residenceName, legalName);
                var pdfBytes = pdfService.GenerateFinancialSummaryPdf(summary);

                var fileName = $"bilan_{request.From}_{request.To}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF");
                return StatusCode(500, "An error occurred while generating the PDF");
            }
        }
    }

    public class BuildingPaymentsReportRequest
    {
        public List<int> Years { get; set; } = new();
    }
}
