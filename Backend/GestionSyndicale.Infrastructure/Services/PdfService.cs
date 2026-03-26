using GestionSyndicale.Core.Interfaces;
using GestionSyndicale.Core.Entities;
using GestionSyndicale.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestionSyndicale.Infrastructure.Services;

public class PdfService : IPdfService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public PdfService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // ─── Labels ───────────────────────────────────────────────────────────────

    private static Dictionary<string, string> GetReceiptLabels(string lang)
    {
        if (lang == "ar")
        {
            return new Dictionary<string, string>
            {
                ["title"]           = "إيصال الاشتراكات",
                ["issued_on"]       = "تاريخ الإصدار",
                ["member_info"]     = "معلومات العضو",
                ["full_name"]       = "الاسم الكامل",
                ["residence"]       = "السكن",
                ["building"]        = "البناية",
                ["apartment"]       = "الشقة",
                ["contributions"]   = "تفاصيل الاشتراكات المدفوعة",
                ["year"]            = "السنة",
                ["month_col"]       = "الشهر",
                ["status_col"]      = "الحالة",
                ["syndic_sig"]      = "توقيع مدير النقابة",
                ["auto_generated"]  = "تم إنشاء هذه الوثيقة تلقائياً",
            };
        }
        return new Dictionary<string, string>
        {
            ["title"]           = "Reçu de Cotisations",
            ["issued_on"]       = "Date d'émission",
            ["member_info"]     = "Informations de l'adhérent",
            ["full_name"]       = "Nom complet",
            ["residence"]       = "Résidence",
            ["building"]        = "Immeuble",
            ["apartment"]       = "Appartement",
            ["contributions"]   = "Détail des cotisations payées",
            ["year"]            = "Année",
            ["month_col"]       = "Mois",
            ["status_col"]      = "Statut",
            ["syndic_sig"]      = "Signature du Syndic",
            ["auto_generated"]  = "Document généré automatiquement",
        };
    }

    // ─── Receipt generation ───────────────────────────────────────────────────

    public async Task<byte[]> GenerateApartmentReceiptAsync(int apartmentId, List<int> years, int userId, string lang = "fr")
    {
        try
        {
            var apartment = await _context.Apartments
                .Include(a => a.Building)
                    .ThenInclude(b => b.Residence)
                .FirstOrDefaultAsync(a => a.Id == apartmentId && !a.IsDeleted);

            if (apartment == null) return Array.Empty<byte>();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ApartmentId == apartmentId && !u.IsDeleted && u.IsActive);

            if (user == null) return Array.Empty<byte>();

            // Build year → set of paid month numbers
            var paidMonthsByYear = new Dictionary<int, HashSet<int>>();
            foreach (var year in years.OrderBy(y => y))
            {
                var paidMonths = await _context.MonthlyPayments
                    .Where(mp => mp.ApartmentId == apartmentId && mp.Year == year && mp.IsPaid)
                    .Select(mp => mp.Month)
                    .ToListAsync();
                paidMonthsByYear[year] = paidMonths.ToHashSet();
            }

            var isRtl         = lang == "ar";
            var labels        = GetReceiptLabels(lang);
            var residenceName = apartment.Building?.Residence?.Name ?? "Résidence";

            // Arabic syndic name from config, fallback to default
            var syndicName = isRtl
                ? (_configuration["Syndic:LegalNameAr"] ?? _configuration["Syndic:LegalName"] ?? "Syndic")
                : (_configuration["Syndic:LegalName"] ?? "Syndic");

            // A4 Landscape — two identical receipts side by side
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(0.6f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    page.Content().Element(content =>
                    {
                        content.Row(row =>
                        {
                            // First receipt (left pane)
                            row.RelativeItem().PaddingRight(6).Element(pane =>
                                ComposeReceipt(pane, apartment, user, paidMonthsByYear,
                                    residenceName, labels, lang, isRtl, syndicName));

                            // Vertical cut line
                            row.ConstantItem(1).Background(Colors.Grey.Lighten1);

                            // Second receipt (right pane) — identical
                            row.RelativeItem().PaddingLeft(6).Element(pane =>
                                ComposeReceipt(pane, apartment, user, paidMonthsByYear,
                                    residenceName, labels, lang, isRtl, syndicName));
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating receipt: {ex.Message}\n{ex.StackTrace}");
            return Array.Empty<byte>();
        }
    }

    // ─── Single receipt pane ──────────────────────────────────────────────────

    private void ComposeReceipt(IContainer container, Apartment apartment, User user,
        Dictionary<int, HashSet<int>> paidMonthsByYear, string residenceName,
        Dictionary<string, string> labels, string lang, bool isRtl, string syndicName)
    {
        container.Column(column =>
        {
            column.Item().Element(h => ComposeHeader(h, labels, isRtl));
            column.Item().PaddingTop(8).Element(c =>
                ComposeContent(c, apartment, user, paidMonthsByYear, residenceName, labels, lang, isRtl));
            column.Item().PaddingTop(8).Element(f => ComposeFooter(f, syndicName, labels, isRtl));
        });
    }

    // ─── Header ───────────────────────────────────────────────────────────────

    private void ComposeHeader(IContainer container, Dictionary<string, string> labels, bool isRtl)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                if (isRtl)
                {
                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().AlignRight().Text(labels["title"])
                            .FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
                        col.Item().PaddingTop(3).AlignRight()
                            .Text($"{labels["issued_on"]} : {DateTime.Now:dd/MM/yyyy}")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                }
                else
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(labels["title"])
                            .FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
                        col.Item().PaddingTop(3)
                            .Text($"{labels["issued_on"]} : {DateTime.Now:dd/MM/yyyy}")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                }
            });

            column.Item().PaddingTop(6).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
        });
    }

    // ─── Content ──────────────────────────────────────────────────────────────

    private void ComposeContent(IContainer container, Apartment apartment, User user,
        Dictionary<int, HashSet<int>> paidMonthsByYear, string residenceName,
        Dictionary<string, string> labels, string lang, bool isRtl)
    {
        container.Column(column =>
        {
            // Identity block
            column.Item().Background(Colors.Grey.Lighten3).Padding(8).Column(info =>
            {
                if (isRtl)
                    info.Item().AlignRight().Text(labels["member_info"]).Bold().FontSize(10);
                else
                    info.Item().Text(labels["member_info"]).Bold().FontSize(10);

                AddInfoRow(info, labels["full_name"], $"{user.FirstName} {user.LastName}", isRtl);
                AddInfoRow(info, labels["residence"], residenceName, isRtl);
                AddInfoRow(info, labels["building"], $"{apartment.Building?.BuildingNumber} - {apartment.Building?.Name}", isRtl);
                AddInfoRow(info, labels["apartment"], apartment.ApartmentNumber, isRtl);
            });

            // Payment section title
            column.Item().PaddingTop(10).Element(e =>
            {
                if (isRtl)
                    e.AlignRight().Text(labels["contributions"]).Bold().FontSize(11);
                else
                    e.Text(labels["contributions"]).Bold().FontSize(11);
            });

            // One table per year
            foreach (var yearEntry in paidMonthsByYear.OrderBy(kv => kv.Key))
            {
                column.Item().PaddingTop(6).Column(yearCol =>
                {
                    yearCol.Item().Element(e =>
                    {
                        if (isRtl)
                            e.AlignRight().Text($"{yearEntry.Key} {labels["year"]}")
                                .Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
                        else
                            e.Text($"{labels["year"]} {yearEntry.Key}")
                                .Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
                    });

                    yearCol.Item().PaddingTop(4).Table(table =>
                    {
                        if (isRtl)
                        {
                            // RTL: Status column on left (visually), Month on right — reads RTL correctly
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(30); // Status (left on paper = left in RTL reading = last)
                                cols.RelativeColumn();   // Month  (right on paper = right in RTL = first)
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(4).AlignCenter()
                                    .Text(labels["status_col"]).FontColor(Colors.White).Bold().FontSize(8);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(4).AlignRight()
                                    .Text(labels["month_col"]).FontColor(Colors.White).Bold().FontSize(8);
                            });

                            for (int month = 1; month <= 12; month++)
                            {
                                bool isPaid = yearEntry.Value.Contains(month);
                                var bg = month % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                                table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(3).AlignCenter()
                                    .Text(isPaid ? "✓" : "✗").FontSize(10).Bold()
                                    .FontColor(isPaid ? Colors.Green.Darken2 : Colors.Red.Darken1);

                                table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(3).AlignRight()
                                    .Text(GetMonthName(month, lang)).FontSize(9);
                            }
                        }
                        else
                        {
                            // LTR: Month on left, Status on right
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn();   // Month
                                cols.ConstantColumn(30); // Status
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(4)
                                    .Text(labels["month_col"]).FontColor(Colors.White).Bold().FontSize(8);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(4).AlignCenter()
                                    .Text(labels["status_col"]).FontColor(Colors.White).Bold().FontSize(8);
                            });

                            for (int month = 1; month <= 12; month++)
                            {
                                bool isPaid = yearEntry.Value.Contains(month);
                                var bg = month % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                                table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(3).Text(GetMonthName(month, lang)).FontSize(9);

                                table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(3).AlignCenter()
                                    .Text(isPaid ? "✓" : "✗").FontSize(10).Bold()
                                    .FontColor(isPaid ? Colors.Green.Darken2 : Colors.Red.Darken1);
                            }
                        }
                    });
                });
            }
        });
    }

    // Helper: one info row, RTL-aware
    private void AddInfoRow(ColumnDescriptor col, string label, string value, bool isRtl)
    {
        col.Item().PaddingTop(4).Row(row =>
        {
            if (isRtl)
            {
                // Physically: value (left) | label (right) → reading RTL: label first, then value ✓
                row.RelativeItem().AlignRight().Text(value).FontSize(8);
                row.RelativeItem().AlignRight().Text($"{label} :").Bold().FontSize(8);
            }
            else
            {
                row.RelativeItem().Text($"{label} :").Bold().FontSize(8);
                row.RelativeItem().Text(value).FontSize(8);
            }
        });
    }

    // ─── Footer ───────────────────────────────────────────────────────────────

    private void ComposeFooter(IContainer container, string syndicName, Dictionary<string, string> labels, bool isRtl)
    {
        container.Column(column =>
        {
            column.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten1);

            column.Item().PaddingTop(8).Element(e =>
            {
                if (isRtl)
                {
                    e.AlignRight().Column(c =>
                    {
                        c.Item().AlignRight().Text(labels["syndic_sig"]).FontSize(8).Italic();
                        c.Item().PaddingTop(4).AlignRight().Text(syndicName).Bold().FontSize(9);
                    });
                }
                else
                {
                    e.Column(c =>
                    {
                        c.Item().Text(labels["syndic_sig"]).FontSize(8).Italic();
                        c.Item().PaddingTop(4).Text(syndicName).Bold().FontSize(9);
                    });
                }
            });

            column.Item().PaddingTop(8).Element(e =>
            {
                if (isRtl)
                    e.AlignRight().Text(labels["auto_generated"]).FontSize(7).FontColor(Colors.Grey.Darken1).Italic();
                else
                    e.Text(labels["auto_generated"]).FontSize(7).FontColor(Colors.Grey.Darken1).Italic();
            });
        });
    }

    // ─── Month names ──────────────────────────────────────────────────────────

    private string GetMonthName(int month, string lang = "fr")
    {
        if (lang == "ar")
        {
            return month switch
            {
                1  => "يناير",  2  => "فبراير", 3  => "مارس",
                4  => "أبريل", 5  => "مايو",   6  => "يونيو",
                7  => "يوليو", 8  => "أغسطس",  9  => "سبتمبر",
                10 => "أكتوبر", 11 => "نوفمبر", 12 => "ديسمبر",
                _  => month.ToString()
            };
        }
        return month switch
        {
            1  => "Janvier",   2  => "Février",  3  => "Mars",
            4  => "Avril",     5  => "Mai",      6  => "Juin",
            7  => "Juillet",   8  => "Août",     9  => "Septembre",
            10 => "Octobre",   11 => "Novembre", 12 => "Décembre",
            _  => month.ToString()
        };
    }

    // ─── Stub methods ─────────────────────────────────────────────────────────

    public async Task<byte[]> GeneratePaymentReceiptAsync(int paymentId)
    { await Task.CompletedTask; return Array.Empty<byte>(); }

    public async Task<byte[]> GenerateMonthlyReportAsync(int year, int month)
    { await Task.CompletedTask; return Array.Empty<byte>(); }

    public async Task<byte[]> GenerateAnnualReportAsync(int year)
    { await Task.CompletedTask; return Array.Empty<byte>(); }

    public async Task<byte[]> GenerateApartmentBalanceStatementAsync(int apartmentId)
    { await Task.CompletedTask; return Array.Empty<byte>(); }

    // ─── Building payments grid ───────────────────────────────────────────────

    public async Task<byte[]> GenerateBuildingPaymentsGridAsync(Building building, List<int> years)
    {
        try
        {
            var apartments = building.Apartments
                .Where(a => !a.IsDeleted)
                .OrderBy(a => int.TryParse(a.ApartmentNumber, out int n) ? n : int.MaxValue)
                .ToList();

            if (!apartments.Any()) return Array.Empty<byte>();

            var apartmentIds = apartments.Select(a => a.Id).ToList();
            var users = await _context.Users
                .Where(u => apartmentIds.Contains(u.ApartmentId ?? 0) && !u.IsDeleted && u.IsActive)
                .ToListAsync();

            var payments = await _context.MonthlyPayments
                .Where(mp => apartmentIds.Contains(mp.ApartmentId) && years.Contains(mp.Year) && mp.IsPaid)
                .ToListAsync();

            var syndicName    = _configuration["Syndic:LegalName"] ?? "Syndic";
            var residenceName = building.Residence?.Name ?? "Résidence";
            var today         = DateTime.Today;
            var currentMonth  = today.Day > 15 ? today.Month : today.Month - 1;
            var currentYear   = today.Year;

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                foreach (var year in years.OrderBy(y => y))
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(0.8f, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                        page.Header().Element(content => ComposeBuildingReportHeader(content, residenceName, building.Name, year));
                        page.Content().Element(content => ComposeBuildingPaymentGridForYear(content, apartments, users, payments, year, currentYear, currentMonth));
                        page.Footer().Element(footer => ComposeBuildingReportFooter(footer, syndicName));
                    });
                }
            });

            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating building payments grid: {ex.Message}");
            return Array.Empty<byte>();
        }
    }

    private void ComposeBuildingReportHeader(IContainer container, string residenceName, string buildingName, int year)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text("ÉTAT DES PAIEMENTS").FontSize(16).Bold();
            column.Item().AlignCenter().PaddingTop(4).Text($"{residenceName} - Immeuble {buildingName}").FontSize(12).SemiBold();
            column.Item().AlignCenter().PaddingTop(2).Text($"Année(s): {year}").FontSize(10);
            column.Item().AlignCenter().PaddingTop(3).Text($"Date d'édition: {DateTime.Now:dd/MM/yyyy}").FontSize(9).Italic();
            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    private void ComposeBuildingPaymentGridForYear(IContainer container, List<Apartment> apartments,
        List<User> users, List<MonthlyPayment> payments, int year, int currentYear, int currentMonth)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(8).Text($"Année {year}").FontSize(12).Bold();

            column.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(55);
                    for (int i = 0; i < 12; i++) columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Appt.").Bold().FontSize(7);
                    header.Cell().Element(CellStyle).Text("Adhérent").Bold().FontSize(7);
                    for (int month = 1; month <= 12; month++)
                        header.Cell().Element(CellStyle).Text(GetMonthNameShort(month)).Bold().FontSize(7);

                    IContainer CellStyle(IContainer c) => c
                        .Border(1).BorderColor(Colors.Grey.Darken2)
                        .Background(Colors.Grey.Lighten3)
                        .Padding(2).AlignCenter().AlignMiddle();
                });

                foreach (var apartment in apartments)
                {
                    table.Cell().Element(CellStyle).Text(apartment.ApartmentNumber).FontSize(7);

                    var user = users.FirstOrDefault(u => u.ApartmentId == apartment.Id);
                    var memberName = user != null
                        ? $"{user.FirstName?.Substring(0, Math.Min(1, user.FirstName?.Length ?? 0))}. {user.LastName?.Substring(0, Math.Min(7, user.LastName?.Length ?? 0))}"
                        : "";
                    table.Cell().Element(CellStyle).Text(memberName).FontSize(6);

                    for (int month = 1; month <= 12; month++)
                    {
                        var isPaid = payments.Any(p => p.ApartmentId == apartment.Id && p.Year == year && p.Month == month);
                        bool isDue = year < currentYear || (year == currentYear && month <= currentMonth);

                        var cell = table.Cell().Element(CellStyle);
                        if (isPaid)     cell.Text("■").FontSize(12).FontColor(Colors.Green.Darken2).Bold();
                        else if (isDue) cell.Text("X").FontSize(10).FontColor(Colors.Red.Darken1).Bold();
                        else            cell.Text("");
                    }

                    IContainer CellStyle(IContainer c) => c
                        .Border(1).BorderColor(Colors.Grey.Medium)
                        .Padding(2).AlignCenter().AlignMiddle();
                }
            });

            column.Item().PaddingTop(6).Row(row =>
            {
                row.AutoItem().Text("Légende: ").FontSize(8).SemiBold();
                row.AutoItem().PaddingLeft(5).Text("■").FontColor(Colors.Green.Darken2).Bold().FontSize(12);
                row.AutoItem().PaddingLeft(3).Text("= Payé").FontSize(8);
                row.AutoItem().PaddingLeft(10).Text("X").FontColor(Colors.Red.Darken1).Bold().FontSize(10);
                row.AutoItem().PaddingLeft(3).Text("= Impayé (dû)").FontSize(8);
                row.AutoItem().PaddingLeft(10).Text("(vide) = Futur").FontSize(8).Italic();
            });
        });
    }

    private void ComposeBuildingReportFooter(IContainer container, string syndicName)
    {
        container.AlignBottom().AlignCenter().Column(column =>
        {
            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
            column.Item().PaddingTop(5).Text($"Document généré par {syndicName}").FontSize(9).Italic();
        });
    }

    private string GetMonthNameShort(int month) => month switch
    {
        1 => "Jan", 2 => "Fév", 3 => "Mar", 4 => "Avr",
        5 => "Mai", 6 => "Jun", 7 => "Jul", 8 => "Aoû",
        9 => "Sep", 10 => "Oct", 11 => "Nov", 12 => "Déc",
        _ => month.ToString()
    };
}
