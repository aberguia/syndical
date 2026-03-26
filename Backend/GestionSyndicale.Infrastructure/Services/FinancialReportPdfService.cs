using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using GestionSyndicale.Core.DTOs.Reports;

namespace GestionSyndicale.Infrastructure.Services;

public class FinancialReportPdfService
{
    private readonly string _residenceName;
    private readonly string _legalName;
    private readonly string _lang;
    private readonly Dictionary<string, string> _L;

    public FinancialReportPdfService(string residenceName, string legalName, string lang = "fr")
    {
        _residenceName = residenceName;
        _legalName     = legalName;
        _lang          = lang;
        _L             = GetLabels(lang);

        QuestPDF.Settings.License = LicenseType.Community;
    }

    // ─── Label dictionary ─────────────────────────────────────────────────────

    private static Dictionary<string, string> GetLabels(string lang)
    {
        if (lang == "ar")
        {
            return new Dictionary<string, string>
            {
                ["report_title"]        = "البيان المالي",
                ["period"]              = "الفترة",
                ["from"]                = "من",
                ["to"]                  = "إلى",
                ["synthesis"]           = "ملخص مالي",
                ["contributions"]       = "إيرادات الاشتراكات",
                ["other_revenues"]      = "إيرادات أخرى",
                ["total_revenues"]      = "إجمالي الإيرادات",
                ["expenses"]            = "النفقات",
                ["net_result"]          = "الصافي",
                ["surplus"]             = "(فائض)",
                ["deficit"]             = "(عجز)",
                ["kpi_revenues"]        = "إجمالي الإيرادات",
                ["kpi_expenses"]        = "إجمالي النفقات",
                ["kpi_rate"]            = "معدل التحصيل",
                ["monthly_evolution"]   = "التطور الشهري",
                ["col_month"]           = "الشهر",
                ["col_contributions"]   = "الاشتراكات",
                ["col_other_rev"]       = "إيرادات أخرى",
                ["col_total_rev"]       = "إجمالي الإيرادات",
                ["col_expenses"]        = "النفقات",
                ["col_result"]          = "النتيجة",
                ["expenses_by_cat"]     = "النفقات حسب الفئة",
                ["col_category"]        = "الفئة",
                ["col_amount"]          = "المبلغ",
                ["revenues_by_type"]    = "الإيرادات الأخرى حسب النوع",
                ["col_title"]           = "العنوان",
                ["generated_on"]        = "تم إنشاؤه في",
            };
        }
        return new Dictionary<string, string>
        {
            ["report_title"]        = "Bilan Financier",
            ["period"]              = "Période",
            ["from"]                = "du",
            ["to"]                  = "au",
            ["synthesis"]           = "SYNTHÈSE FINANCIÈRE",
            ["contributions"]       = "Revenus des cotisations",
            ["other_revenues"]      = "Autres revenus",
            ["total_revenues"]      = "Total revenus",
            ["expenses"]            = "Dépenses",
            ["net_result"]          = "RÉSULTAT NET",
            ["surplus"]             = "(Excédent)",
            ["deficit"]             = "(Déficit)",
            ["kpi_revenues"]        = "Total Revenus",
            ["kpi_expenses"]        = "Total Dépenses",
            ["kpi_rate"]            = "Taux de recouvrement",
            ["monthly_evolution"]   = "ÉVOLUTION MENSUELLE",
            ["col_month"]           = "Mois",
            ["col_contributions"]   = "Cotisations",
            ["col_other_rev"]       = "Autres Rev.",
            ["col_total_rev"]       = "Total Rev.",
            ["col_expenses"]        = "Dépenses",
            ["col_result"]          = "Résultat",
            ["expenses_by_cat"]     = "Dépenses par catégorie",
            ["col_category"]        = "Catégorie",
            ["col_amount"]          = "Montant",
            ["revenues_by_type"]    = "Autres revenus par type",
            ["col_title"]           = "Titre",
            ["generated_on"]        = "Généré le",
        };
    }

    // ─── Month names ──────────────────────────────────────────────────────────

    private string[] GetMonthNames()
    {
        if (_lang == "ar")
            return new[] { "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
                           "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };

        return new[] { "Janvier", "Février", "Mars", "Avril", "Mai", "Juin",
                       "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre" };
    }

    // ─── Public entry point ───────────────────────────────────────────────────

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
                    text.Span($"{_L["generated_on"]} ").FontSize(9).FontColor(Colors.Grey.Medium);
                    text.Span(DateTime.Now.ToString("dd/MM/yyyy à HH:mm")).FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });
        });

        return document.GeneratePdf();
    }

    // ─── Sections ─────────────────────────────────────────────────────────────

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text(_residenceName)
                .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);

            column.Item().AlignCenter().Text(_legalName)
                .FontSize(12).FontColor(Colors.Grey.Darken1);

            column.Item().PaddingTop(10).AlignCenter().Text(_L["report_title"])
                .FontSize(16).Bold().FontColor(Colors.Grey.Darken3);

            column.Item().PaddingTop(5).AlignCenter().LineHorizontal(2).LineColor(Colors.Blue.Darken2);
        });
    }

    private void ComposeContent(IContainer container, FinancialSummaryDto summary)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(15);

            column.Item().Text(text =>
            {
                text.Span($"{_L["period"]} : ").Bold();
                text.Span($"{_L["from"]} {FormatDate(summary.From)} {_L["to"]} {FormatDate(summary.To)}");
            });

            column.Item().Element(c => ComposeSynthesis(c, summary.Totals));
            column.Item().Element(c => ComposeKPIs(c, summary.Totals, summary.CollectionRate));

            if (summary.ByMonth.Count > 0)
                column.Item().Element(c => ComposeMonthlyTable(c, summary.ByMonth));

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
            column.Item().Text(_L["synthesis"]).Bold().FontSize(14);

            column.Item().PaddingLeft(10).Column(inner =>
            {
                inner.Spacing(5);

                inner.Item().Text(text =>
                {
                    text.Span($"{_L["contributions"]} : ").FontColor(Colors.Grey.Darken1);
                    text.Span(FormatAmount(totals.Contributions)).Bold().FontColor(Colors.Green.Darken1);
                });

                inner.Item().Text(text =>
                {
                    text.Span($"{_L["other_revenues"]} : ").FontColor(Colors.Grey.Darken1);
                    text.Span(FormatAmount(totals.OtherRevenues)).Bold().FontColor(Colors.Green.Darken1);
                });

                inner.Item().BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingTop(5).Text(text =>
                {
                    text.Span($"{_L["total_revenues"]} : ").Bold();
                    text.Span(FormatAmount(totals.TotalRevenues)).Bold().FontSize(12).FontColor(Colors.Green.Darken2);
                });

                inner.Item().PaddingTop(5).Text(text =>
                {
                    text.Span($"{_L["expenses"]} : ").Bold();
                    text.Span(FormatAmount(totals.Expenses)).Bold().FontSize(12).FontColor(Colors.Red.Darken1);
                });

                inner.Item().BorderTop(2).BorderColor(Colors.Grey.Darken2).PaddingTop(8).Text(text =>
                {
                    text.Span($"{_L["net_result"]} : ").Bold().FontSize(13);
                    text.Span(FormatAmount(totals.NetResult)).Bold().FontSize(13)
                        .FontColor(totals.NetResult >= 0 ? Colors.Green.Darken2 : Colors.Orange.Darken2);
                    text.Span(totals.NetResult >= 0 ? $" {_L["surplus"]}" : $" {_L["deficit"]}")
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
                c.Item().Text(_L["kpi_revenues"]).FontSize(9).FontColor(Colors.Grey.Darken1);
                c.Item().Text(FormatAmount(totals.TotalRevenues)).Bold().FontSize(12).FontColor(Colors.Green.Darken2);
            });

            row.Spacing(10);

            row.RelativeItem().Background(Colors.Red.Lighten4).Padding(10).Column(c =>
            {
                c.Item().Text(_L["kpi_expenses"]).FontSize(9).FontColor(Colors.Grey.Darken1);
                c.Item().Text(FormatAmount(totals.Expenses)).Bold().FontSize(12).FontColor(Colors.Red.Darken2);
            });

            row.Spacing(10);

            row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(c =>
            {
                c.Item().Text(_L["kpi_rate"]).FontSize(9).FontColor(Colors.Grey.Darken1);
                c.Item().Text($"{collectionRate.Rate:F1}%").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
            });
        });
    }

    private void ComposeMonthlyTable(IContainer container, List<MonthlyFinancialDto> monthlyData)
    {
        container.Column(column =>
        {
            column.Item().PaddingBottom(5).Text(_L["monthly_evolution"]).Bold().FontSize(12);

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

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text(_L["col_month"]).FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text(_L["col_contributions"]).FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text(_L["col_other_rev"]).FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text(_L["col_total_rev"]).FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text(_L["col_expenses"]).FontColor(Colors.White).Bold().FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text(_L["col_result"]).FontColor(Colors.White).Bold().FontSize(9);
                });

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
            column.Item().PaddingBottom(5).Text(_L["expenses_by_cat"]).Bold().FontSize(11);

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
                    header.Cell().Background(Colors.Red.Lighten3).Padding(4).Text(_L["col_category"]).Bold().FontSize(9);
                    header.Cell().Background(Colors.Red.Lighten3).Padding(4).Text(_L["col_amount"]).Bold().FontSize(9);
                    header.Cell().Background(Colors.Red.Lighten3).Padding(4).Text("%").Bold().FontSize(9);
                });

                foreach (var expense in expenses)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(expense.CategoryName).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(FormatAmount(expense.Amount)).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"{expense.Percent:F1}%").FontSize(8);
                }
            });
        });
    }

    private void ComposeRevenuesByTitle(IContainer container, List<RevenueByTitleDto> revenues)
    {
        if (revenues.Count == 0) return;

        container.Column(column =>
        {
            column.Item().PaddingBottom(5).Text(_L["revenues_by_type"]).Bold().FontSize(11);

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
                    header.Cell().Background(Colors.Green.Lighten3).Padding(4).Text(_L["col_title"]).Bold().FontSize(9);
                    header.Cell().Background(Colors.Green.Lighten3).Padding(4).Text(_L["col_amount"]).Bold().FontSize(9);
                    header.Cell().Background(Colors.Green.Lighten3).Padding(4).Text("%").Bold().FontSize(9);
                });

                foreach (var revenue in revenues)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(revenue.Title).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(FormatAmount(revenue.Amount)).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"{revenue.Percent:F1}%").FontSize(8);
                }
            });
        });
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private string FormatAmount(decimal amount) => $"{amount:N2} MAD";

    private string FormatDate(string dateStr)
    {
        if (DateTime.TryParse(dateStr, out var date))
            return date.ToString("dd/MM/yyyy");
        return dateStr;
    }

    private string FormatMonth(string monthStr)
    {
        var parts = monthStr.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[1], out var month))
        {
            var names = GetMonthNames();
            return $"{names[month - 1]} {parts[0]}";
        }
        return monthStr;
    }
}
