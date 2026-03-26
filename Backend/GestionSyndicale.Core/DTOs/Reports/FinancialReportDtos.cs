namespace GestionSyndicale.Core.DTOs.Reports;

public class FinancialSummaryDto
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public FinancialTotalsDto Totals { get; set; } = new();
    public List<MonthlyFinancialDto> ByMonth { get; set; } = new();
    public List<ExpenseByCategoryDto> ExpensesByCategory { get; set; } = new();
    public List<RevenueByTitleDto> OtherRevenuesByTitle { get; set; } = new();
    public CollectionRateDto CollectionRate { get; set; } = new();
}

public class FinancialTotalsDto
{
    public decimal Contributions { get; set; }
    public decimal OtherRevenues { get; set; }
    public decimal TotalRevenues { get; set; }
    public decimal Expenses { get; set; }
    public decimal NetResult { get; set; }
}

public class MonthlyFinancialDto
{
    public string Month { get; set; } = string.Empty; // "YYYY-MM"
    public decimal Contributions { get; set; }
    public decimal OtherRevenues { get; set; }
    public decimal TotalRevenues { get; set; }
    public decimal Expenses { get; set; }
    public decimal NetResult { get; set; }
}

public class ExpenseByCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percent { get; set; }
}

public class RevenueByTitleDto
{
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percent { get; set; }
}

public class CollectionRateDto
{
    public decimal ExpectedAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal Rate { get; set; } // Percentage 0-100
}

public class GeneratePdfRequest
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Lang { get; set; } = "fr";
}
