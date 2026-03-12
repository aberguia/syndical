using GestionSyndicale.Core.Interfaces;
using GestionSyndicale.Core.Entities;
using GestionSyndicale.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestionSyndicale.Infrastructure.Services;

/// <summary>
/// Service de génération de PDF avec QuestPDF
/// À implémenter selon les besoins spécifiques
/// </summary>
public class PdfService : IPdfService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public PdfService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        
        // Configuration QuestPDF pour usage communautaire
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateApartmentReceiptAsync(int apartmentId, List<int> years, int userId)
    {
        try
        {
            // Récupérer les données de l'appartement
            var apartment = await _context.Apartments
                .Include(a => a.Building)
                    .ThenInclude(b => b.Residence)
                .FirstOrDefaultAsync(a => a.Id == apartmentId && !a.IsDeleted);

            if (apartment == null)
            {
                Console.WriteLine($"Apartment {apartmentId} not found");
                return Array.Empty<byte>();
            }

            // Récupérer l'adhérent via la relation User.ApartmentId
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ApartmentId == apartmentId && !u.IsDeleted && u.IsActive);

            if (user == null)
            {
                Console.WriteLine($"No user found for apartment {apartmentId}");
                return Array.Empty<byte>();
            }

            Console.WriteLine($"Found user {user.FirstName} {user.LastName} linked to apartment {apartmentId}");

            // Récupérer les mois payés pour toutes les années demandées
            var paidMonthsByYear = new Dictionary<int, List<string>>();
            
            foreach (var year in years.OrderBy(y => y))
            {
                var paidMonths = await _context.MonthlyPayments
                    .Where(mp => mp.ApartmentId == apartmentId && mp.Year == year && mp.IsPaid)
                    .Select(mp => mp.Month)
                    .OrderBy(m => m)
                    .ToListAsync();

                paidMonthsByYear[year] = paidMonths.Select(m => GetMonthName(m)).ToList();
            }

            // Configuration
            var syndicName = _configuration["Syndic:LegalName"] ?? "Syndic";
            var residenceName = apartment.Building?.Residence?.Name ?? "Résidence";

            // Générer le PDF
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(content => ComposeContent(content, apartment, user, paidMonthsByYear, residenceName));
                    page.Footer().Element(footer => ComposeFooter(footer, syndicName));
                });
            });

            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            // Log l'erreur si nécessaire
            Console.WriteLine($"Error generating receipt: {ex.Message}");
            Console.WriteLine($"Error generating receipt: {ex.Message}");
            return Array.Empty<byte>();
        }
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Reçu de Cotisations")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);

                    col.Item().PaddingTop(5).Text($"Date d'émission : {DateTime.Now:dd/MM/yyyy}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                });
            });

            column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
        });
    }

    private void ComposeContent(IContainer container, Apartment apartment, User user, Dictionary<int, List<string>> paidMonthsByYear, string residenceName)
    {
        container.PaddingTop(20).Column(column =>
        {
            // Bloc identité
            column.Item().Background(Colors.Grey.Lighten3).Padding(15).Column(infoColumn =>
            {
                infoColumn.Item().Text("Informations de l'adhérent").Bold().FontSize(12);
                infoColumn.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Text($"Nom complet : ").Bold();
                    row.RelativeItem().Text($"{user.FirstName} {user.LastName}");
                });
                infoColumn.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text($"Résidence : ").Bold();
                    row.RelativeItem().Text(residenceName);
                });
                infoColumn.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text($"Immeuble : ").Bold();
                    row.RelativeItem().Text($"{apartment.Building?.BuildingNumber} - {apartment.Building?.Name}");
                });
                infoColumn.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text($"Appartement : ").Bold();
                    row.RelativeItem().Text(apartment.ApartmentNumber);
                });
            });

            // Bloc paiements par année
            column.Item().PaddingTop(30).Text("Détail des cotisations payées").Bold().FontSize(14);

            foreach (var yearEntry in paidMonthsByYear.OrderBy(kv => kv.Key))
            {
                column.Item().PaddingTop(15).Column(yearColumn =>
                {
                    yearColumn.Item().Text($"Année {yearEntry.Key}").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                    
                    if (yearEntry.Value.Any())
                    {
                        yearColumn.Item().PaddingTop(5).Text($"Mois payés : {string.Join(", ", yearEntry.Value)}")
                            .FontSize(11);
                    }
                    else
                    {
                        yearColumn.Item().PaddingTop(5).Text("Aucun paiement enregistré")
                            .FontSize(11)
                            .Italic()
                            .FontColor(Colors.Grey.Darken1);
                    }
                });
            }
        });
    }

    private void ComposeFooter(IContainer container, string syndicName)
    {
        container.AlignBottom().Column(column =>
        {
            column.Item().PaddingTop(30).BorderTop(1).BorderColor(Colors.Grey.Lighten1);
            
            column.Item().PaddingTop(15).Row(row =>
            {
                row.RelativeItem().Column(signColumn =>
                {
                    signColumn.Item().Text("Signature du Syndic").FontSize(10).Italic();
                    signColumn.Item().PaddingTop(5).Text(syndicName).Bold().FontSize(11);
                });
            });

            column.Item().PaddingTop(20).Text("Document généré automatiquement")
                .FontSize(8)
                .FontColor(Colors.Grey.Darken1)
                .Italic();
        });
    }

    private string GetMonthName(int month)
    {
        return month switch
        {
            1 => "Janvier",
            2 => "Février",
            3 => "Mars",
            4 => "Avril",
            5 => "Mai",
            6 => "Juin",
            7 => "Juillet",
            8 => "Août",
            9 => "Septembre",
            10 => "Octobre",
            11 => "Novembre",
            12 => "Décembre",
            _ => month.ToString()
        };
    }

    // Autres méthodes existantes
    public async Task<byte[]> GeneratePaymentReceiptAsync(int paymentId)
    {
        // TODO: Implémenter avec QuestPDF ou alternative
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }

    public async Task<byte[]> GenerateMonthlyReportAsync(int year, int month)
    {
        // TODO: Implémenter rapport mensuel PDF
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }

    public async Task<byte[]> GenerateAnnualReportAsync(int year)
    {
        // TODO: Implémenter rapport annuel PDF
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }

    public async Task<byte[]> GenerateApartmentBalanceStatementAsync(int apartmentId)
    {
        // TODO: Implémenter relevé de compte appartement PDF
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }

    public async Task<byte[]> GenerateBuildingPaymentsGridAsync(Building building, List<int> years)
    {
        try
        {
            // Récupérer tous les appartements avec leurs paiements
            var apartments = building.Apartments
                .Where(a => !a.IsDeleted)
                .OrderBy(a =>
                {
                    // Tri numérique si possible, sinon alphabétique
                    if (int.TryParse(a.ApartmentNumber, out int numericValue))
                        return numericValue;
                    return int.MaxValue; // Mettre les non-numériques à la fin
                })
                .ToList();

            if (!apartments.Any())
            {
                Console.WriteLine($"No apartments found in building {building.Id}");
                return Array.Empty<byte>();
            }

            // Récupérer les users liés aux appartements (via User.ApartmentId)
            var apartmentIds = apartments.Select(a => a.Id).ToList();
            var users = await _context.Users
                .Where(u => apartmentIds.Contains(u.ApartmentId ?? 0) && !u.IsDeleted && u.IsActive)
                .ToListAsync();

            // Récupérer tous les paiements pour ces appartements
            var payments = await _context.MonthlyPayments
                .Where(mp => apartmentIds.Contains(mp.ApartmentId) && 
                             years.Contains(mp.Year) && 
                             mp.IsPaid)
                .ToListAsync();

            // Configuration
            var syndicName = _configuration["Syndic:LegalName"] ?? "Syndic";
            var residenceName = building.Residence?.Name ?? "Résidence";

            // Calculer les mois dus selon la règle du 15
            var today = DateTime.Today;
            var currentMonth = today.Day > 15 ? today.Month : today.Month - 1;
            var currentYear = today.Year;

            // Générer le PDF - une page par année
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                foreach (var year in years.OrderBy(y => y))
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(0.8f, Unit.Centimetre);
                        page.PageColor(Colors.White);
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
            column.Item().AlignCenter().Text("ÉTAT DES PAIEMENTS")
                .FontSize(16)
                .Bold();

            column.Item().AlignCenter().PaddingTop(4).Text($"{residenceName} - Immeuble {buildingName}")
                .FontSize(12)
                .SemiBold();

            column.Item().AlignCenter().PaddingTop(2).Text($"Année(s): {year}")
                .FontSize(10);

            column.Item().AlignCenter().PaddingTop(3).Text($"Date d'édition: {DateTime.Now:dd/MM/yyyy}")
                .FontSize(9)
                .Italic();

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    private void ComposeBuildingPaymentGridForYear(IContainer container, List<Apartment> apartments, 
        List<User> users, List<MonthlyPayment> payments, int year, int currentYear, int currentMonth)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(8).Text($"Année {year}")
                .FontSize(12)
                .Bold();

            column.Item().PaddingTop(4).Table(table =>
                {
                    // Colonnes: Appartement + 12 mois
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50); // Appartement
                        columns.ConstantColumn(55); // Adhérent
                        for (int i = 0; i < 12; i++)
                        {
                            columns.RelativeColumn(); // Mois
                        }
                    });

                    // En-tête
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Appt.").Bold().FontSize(7);
                        header.Cell().Element(CellStyle).Text("Adhérent").Bold().FontSize(7);
                        for (int month = 1; month <= 12; month++)
                        {
                            header.Cell().Element(CellStyle).Text(GetMonthNameShort(month)).Bold().FontSize(7);
                        }

                        IContainer CellStyle(IContainer c) => c
                            .Border(1)
                            .BorderColor(Colors.Grey.Darken2)
                            .Background(Colors.Grey.Lighten3)
                            .Padding(2)
                            .AlignCenter()
                            .AlignMiddle();
                    });

                    // Lignes des appartements
                    foreach (var apartment in apartments)
                    {
                        table.Cell().Element(CellStyle).Text(apartment.ApartmentNumber).FontSize(7);
                        
                        // Nom adhérent via User.ApartmentId
                        var user = users.FirstOrDefault(u => u.ApartmentId == apartment.Id);
                        var memberName = user != null 
                            ? $"{user.FirstName?.Substring(0, Math.Min(1, user.FirstName?.Length ?? 0))}. {user.LastName?.Substring(0, Math.Min(7, user.LastName?.Length ?? 0))}"
                            : "";
                        table.Cell().Element(CellStyle).Text(memberName).FontSize(6);

                        // Cellules pour chaque mois
                        for (int month = 1; month <= 12; month++)
                        {
                            var isPaid = payments.Any(p => p.ApartmentId == apartment.Id && 
                                                           p.Year == year && 
                                                           p.Month == month);

                            // Déterminer si le mois est dû
                            bool isDue = false;
                            if (year < currentYear)
                            {
                                isDue = true; // Toutes les années passées sont dues
                            }
                            else if (year == currentYear)
                            {
                                isDue = month <= currentMonth; // Mois jusqu'au mois courant (règle du 15)
                            }

                            var cell = table.Cell().Element(CellStyle);

                            if (isPaid)
                            {
                                cell.Text("■").FontSize(12).FontColor(Colors.Green.Darken2).Bold();
                            }
                            else if (isDue)
                            {
                                cell.Text("X").FontSize(10).FontColor(Colors.Red.Darken1).Bold();
                            }
                            else
                            {
                                cell.Text(""); // Future months
                            }
                        }

                        IContainer CellStyle(IContainer c) => c
                            .Border(1)
                            .BorderColor(Colors.Grey.Medium)
                            .Padding(2)
                            .AlignCenter()
                            .AlignMiddle();
                    }
                });

            // Légende
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
            
            column.Item().PaddingTop(5).Text($"Document généré par {syndicName}")
                .FontSize(9)
                .Italic();
        });
    }

    private string GetMonthNameShort(int month)
    {
        return month switch
        {
            1 => "Jan",
            2 => "Fév",
            3 => "Mar",
            4 => "Avr",
            5 => "Mai",
            6 => "Jun",
            7 => "Jul",
            8 => "Aoû",
            9 => "Sep",
            10 => "Oct",
            11 => "Nov",
            12 => "Déc",
            _ => month.ToString()
        };
    }
}
