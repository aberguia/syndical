namespace GestionSyndicale.Core.DTOs.Expense;

/// <summary>
/// DTO pour lister les catégories
/// </summary>
public class ExpenseCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ExpensesCount { get; set; }
}

/// <summary>
/// DTO pour créer une catégorie
/// </summary>
public class CreateExpenseCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO pour modifier une catégorie
/// </summary>
public class UpdateExpenseCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
