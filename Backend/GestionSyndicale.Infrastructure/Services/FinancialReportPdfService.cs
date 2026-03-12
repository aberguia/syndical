using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using GestionSyndicale.Core.DTOs.Reports;

namespace GestionSyndicale.Infrastructure.Services;

public class FinancialReportPdfService
{
    private readonly string _residenceName;
    private readonly string _legalName;

    public FinancialReportPdfService(string residenceName, string legalName)
    {
        _residenceName = residenceName;
        _legalName = legalName;
        
        // Configure QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateFinancialSummaryPdf(FinancialSummaryDto summary)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(content => ComposeContent(content, summary));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Généré le ").FontSize(9).FontColor(Colors.Grey.Medium);
                    text.Span(DateTime.Now.ToString("dd/MM/yyyy à HH:mm")).FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text(_residenceName)
                .FontSize(18)
                .Bold()
                .FontColor(Colors.Blue.Darken2);

            column.Item().AlignCenter().Text(_legalName)
                .FontSize(12)
                .FontColor(Colors.Grey.Darken1);

            column.Item().PaddingTop(10).AlignCenter().Text("Bilan Financier")
                .FontSize(16)
                .Bold()
                .FontColor(Colors.Grey.Darken3);

            column.Item().PaddingTop(5).AlignCenter().LineHorizontal(2)
                .LineColor(Colors.Blue.Darken2);
        });
    }

    private void ComposeContent(IContainer container, FinancialSummaryDto summary)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(15);

            // Période
            column.Item().Text(text =>
            {
                text.Span("Période : ").Bold();
                text.Span($"du {FormatDate(summary.From)} au {FormatDate(summary.To)}");
            });

            // Synthèse
            column.Item().Element(c => ComposeSynthesis(c, summary.Totals));

            // KPIs en cartes
            column.Item().Element(c => ComposeKPIs(c, summary.Totals, summary.CollectionRate));

            // Évolution mensuelle
            if (summary.ByMonth.Count > 0)
            {
                column.Item().Element(c => ComposeMonthlyTable(c, summary.ByMonth));
            }

            // Ventilation
            column.Item().Row(row =>
            {
                row.RelativeItem().Element(c => ComposeExpensesByCategory(c, summary.ExpensesByCategory));
                row.RelativeItem().PaddingLeft(10).Element(c => ComposeRevenuesByTitle(c, summary.OtherRevenuesByTitle));
            });
        });
    }

    private void ComposeSynthesis(IContainer container, FinancialTotalsDto totals)
    {
        container.Background(Colors.Grey.Lighten3).Padding(15).Column(column =>
        {
            column.Spacing(8);

            column.Item().Text("SYNTHÈSE FINANCIÈRE").Bold().FontSize(14);

            column.Item().PaddingLeft(10).Column(inner =>
            {
                inner.Spacing(5);
                
                inner.Item().Text(text =>
                {
                    text.Span("Revenus des cotisations : ").FontColor(Colors.Grey.Darken1);
                    text.Span(FormatAmount(totals.Contributions)).Bold().FontColor(Colors.Green.Darken1);
                });

                inner.Item().Text(text =>
                {
                    text.Span("Autres revenus : ").FontColor(Colors.Grey.Darken1);
                    text.Span(FormatAmount(totals.OtherRevenues)).Bold().FontColor(Colors.Green.Darken1);
                });

                inner.Item().BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingTop(5).Text(text =>
                {
                    text.Span("Total revenus : ").Bold();
                    text.Span(FormatAmount(totals.TotalRevenues)).Bold().FontSize(12).FontColor(Colors.Green.Darken2);
                });

                inner.Item().PaddingTop(5).Text(text =>
                {
                    text.Span("Dépenses : ").Bold();
                    text.Span(FormatAmount(totals.Expenses)).Bold().FontSize(12).FontColor(Colors.Red.Darken1);
                });

                inner.Item().BorderTop(2).BorderColor(Colors.Grey.Darken2).PaddingTop(8).Text(text =>
                {
                    text.Span("RÉSULTAT NET : ").Bold().FontSize(13);
                    text.Span(FormatAmount(totals.NetResult)).Bold().FontSize(13)
                        .FontColor(totals.NetResult >= 0 ? Colors.Green.Darken2 : Colors.Orange.Darken2);
                    text.Span(totals.NetResult >= 0 ? " (Excédent)" : " (Déficit)")
                        .FontSize(11).Italic().FontColor(Colors.Grey.Darken1);
                });
            });
        });
    }

    private void ComposeKPIs(IContainer container, FinancialTotalsDto totals, CollectionRateDto collectionRate)
    {
        container.Row(row =>
        {
            row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(c =>
            {
                c.Item().Text("Total Revenus").FontSize(9).FontColor(Colors.Grey.Darken1);
                c.Item().Text(FormatAmount(totals.TotalRevenues)).Bold().FontSize(12).FontColor(Colors.Green.Darken2);
            });

            row.Spacing(10);

            row.RelativeItem().Background(Colors.Red.Lighten4).Padding(10).Column(c =>
            {
                c.Item().Text("Total Dépenses").FontSize(9).FontColor(Colors.Grey.Darken1);
                c.Item().Text(FormatAmount(totals.Expenses)).Bold().FontSize(12).FontColor(Colors.Red.Darken2);
            });

            row.Spacing(10);

            row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(c =>
            {
                c.Item().Text("Taux de recouvrement").FontSize(9).FontColor(Colors.Grey.Darken1);
                c.Item().Text($"{collectionRate.Rate:F1}%").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
            });
        });
    }

    private void ComposeMonthlyTable(IContainer container, List<MonthlyFinancialDto> monthlyData)
    {
        container.Column(column =>
        {
            column.Item().PaddingBottom(5).Text("ÉVOLUTION MENSUELLE").Bold().FontSize(12);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.5f);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Mois").FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Cotisations").FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Autres Rev.").FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Total Rev.").FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Dépenses").FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Résultat").FontColor(Colors.White).Bold().FontSize(9);
                });

                // Rows
                foreach (var month in monthlyData)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(FormatMonth(month.Month)).FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(FormatAmount(month.Contributions)).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(FormatAmount(month.OtherRevenues)).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Background(Colors.Grey.Lighten4)
                        .Text(FormatAmount(month.TotalRevenues)).FontSize(8).Bold();
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(FormatAmount(month.Expenses)).FontSize(8).FontColor(Colors.Red.Darken1);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(FormatAmount(month.NetResult)).FontSize(8).Bold()
                        .FontColor(month.NetResult >= 0 ? Colors.Green.Darken2 : Colors.Orange.Darken2);
                }
            });
        });
    }

    private void ComposeExpensesByCategory(IContainer container, List<ExpenseByCategoryDto> expenses)
    {
        if (expenses.Count == 0) return;

        container.Column(column =>
        {
            column.Item().PaddingBottom(5).Text("Dépenses par catégorie").Bold().FontSize(11);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Red.Lighten3).Padding(4).Text("Catégorie").Bold().FontSize(9);
                    header.Cell().Background(Colors.Red.Lighten3).Padding(4).Text("Montant").Bold().FontSize(9);
                    header.Cell().Background(Colors.Red.Lighten3).Padding(4).Text("%").Bold().FontSize(9);
                });

                foreach (var expense in expenses)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text(expense.CategoryName).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text(FormatAmount(expense.Amount)).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text($"{expense.Percent:F1}%").FontSize(8);
                }
            });
        });
    }

    private void ComposeRevenuesByTitle(IContainer container, List<RevenueByTitleDto> revenues)
    {
        if (revenues.Count == 0) return;

        container.Column(column =>
        {
            column.Item().PaddingBottom(5).Text("Autres revenus par type").Bold().FontSize(11);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Green.Lighten3).Padding(4).Text("Titre").Bold().FontSize(9);
                    header.Cell().Background(Colors.Green.Lighten3).Padding(4).Text("Montant").Bold().FontSize(9);
                    header.Cell().Background(Colors.Green.Lighten3).Padding(4).Text("%").Bold().FontSize(9);
                });

                foreach (var revenue in revenues)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text(revenue.Title).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text(FormatAmount(revenue.Amount)).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                        .Text($"{revenue.Percent:F1}%").FontSize(8);
                }
            });
        });
    }

    private string FormatAmount(decimal amount)
    {
        return $"{amount:N2} MAD";
    }

    private string FormatDate(string dateStr)
    {
        if (DateTime.TryParse(dateStr, out var date))
        {
            return date.ToString("dd/MM/yyyy");
        }
        return dateStr;
    }

    private string FormatMonth(string monthStr)
    {
        var parts = monthStr.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[1], out var month))
        {
            var monthNames = new[] { "Janvier", "Février", "Mars", "Avril", "Mai", "Juin", 
                                     "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre" };
            return $"{monthNames[month - 1]} {parts[0]}";
        }
        return monthStr;
    }
}
