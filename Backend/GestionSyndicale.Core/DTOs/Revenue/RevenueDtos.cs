namespace GestionSyndicale.Core.DTOs.Revenue;

/// <summary>
/// Matrice des cotisations mensuelles par immeuble
/// </summary>
public class ContributionsMatrixDto
{
    public int Year { get; set; }
    public List<BuildingMonthlyContributionsDto> Buildings { get; set; } = new();
    public List<decimal> MonthlyTotals { get; set; } = new(); // Total par mois (12 valeurs)
    public decimal YearTotal { get; set; }
    public decimal MonthlyContributionAmount { get; set; } // Depuis config
}

public class BuildingMonthlyContributionsDto
{
    public int BuildingId { get; set; }
    public string BuildingCode { get; set; } = string.Empty;
    public List<decimal> MonthlyAmounts { get; set; } = new(); // 12 valeurs (Jan=0, Dec=11)
    public decimal RowTotal { get; set; }
}

/// <summary>
/// Liste paginée des autres revenus
/// </summary>
public class OtherRevenuePagedResultDto
{
    public List<OtherRevenueListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class OtherRevenueListDto
{
    public int Id { get; set; }
    public DateTime RevenueDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public int AttachmentsCount { get; set; }
    public DateTime RecordedAt { get; set; }
    public string RecordedByName { get; set; } = string.Empty;
}

public class OtherRevenueDetailDto
{
    public int Id { get; set; }
    public DateTime RevenueDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime RecordedAt { get; set; }
    public int RecordedByUserId { get; set; }
    public string RecordedByName { get; set; } = string.Empty;
    public List<DocumentDto> Attachments { get; set; } = new();
}

public class DocumentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
}

public class CreateOtherRevenueDto
{
    public DateTime RevenueDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public class UpdateOtherRevenueDto
{
    public DateTime RevenueDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Totaux globaux pour le bandeau
/// </summary>
public class RevenuesTotalDto
{
    public int Year { get; set; }
    public decimal ContributionsTotal { get; set; }
    public decimal OtherRevenuesTotal { get; set; }
    public decimal GrandTotal { get; set; }
}
