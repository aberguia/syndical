using GestionSyndicale.Core.DTOs;
using GestionSyndicale.Core.DTOs.Payment;
using GestionSyndicale.Core.Entities;
using GestionSyndicale.Core.Interfaces;
using GestionSyndicale.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionSyndicale.Infrastructure.Services;

/// <summary>
/// Service de gestion des paiements avec transactions
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IPdfService _pdfService;
    private readonly INotificationService _notificationService;

    public PaymentService(
        ApplicationDbContext context, 
        IEmailService emailService, 
        IPdfService pdfService,
        INotificationService notificationService)
    {
        _context = context;
        _emailService = emailService;
        _pdfService = pdfService;
        _notificationService = notificationService;
    }

    public async Task<(bool Success, int PaymentId, string Message)> RecordPaymentAsync(CreatePaymentDto dto, int recordedByUserId)
    {
        // Transaction pour garantir l'intégrité
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Vérifier que l'appartement existe
            var apartment = await _context.Apartments
                .Include(a => a.Building)
                .Include(a => a.PrimaryOwner)
                .FirstOrDefaultAsync(a => a.Id == dto.ApartmentId);

            if (apartment == null)
            {
                return (false, 0, "Appartement introuvable.");
            }

            // Créer le paiement
            var payment = new Payment
            {
                ApartmentId = dto.ApartmentId,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                ReferenceNumber = dto.ReferenceNumber,
                PaymentDate = dto.PaymentDate,
                RecordedAt = DateTime.UtcNow,
                RecordedByUserId = recordedByUserId,
                Notes = dto.Notes
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Récupérer les appels de fonds en attente (les plus anciens en premier)
            var pendingCalls = await _context.CallsForFunds
                .Where(c => c.ApartmentId == dto.ApartmentId && c.AmountRemaining > 0)
                .OrderBy(c => c.DueDate)
                .ToListAsync();

            // Allouer le paiement aux appels de fonds
            decimal remainingAmount = dto.Amount;

            foreach (var call in pendingCalls)
            {
                if (remainingAmount <= 0) break;

                decimal allocationAmount = Math.Min(remainingAmount, call.AmountRemaining);

                // Créer l'allocation
                var allocation = new PaymentAllocation
                {
                    PaymentId = payment.Id,
                    CallForFundsId = call.Id,
                    AllocatedAmount = allocationAmount,
                    AllocatedAt = DateTime.UtcNow
                };

                _context.PaymentAllocations.Add(allocation);

                // Mettre à jour l'appel de fonds
                call.AmountPaid += allocationAmount;
                call.AmountRemaining -= allocationAmount;
                call.UpdatedAt = DateTime.UtcNow;

                // Mettre à jour le statut
                if (call.AmountRemaining == 0)
                {
                    call.Status = "Paid";
                }
                else if (call.AmountPaid > 0)
                {
                    call.Status = "PartiallyPaid";
                }

                remainingAmount -= allocationAmount;
            }

            await _context.SaveChangesAsync();

            // Générer le reçu PDF
            var receiptPdf = await _pdfService.GeneratePaymentReceiptAsync(payment.Id);
            var receiptFileName = $"Recu_{payment.Id}_{DateTime.Now:yyyyMMdd}.pdf";
            var receiptPath = Path.Combine("Uploads", "Receipts", receiptFileName);
            
            // Sauvegarder le PDF
            Directory.CreateDirectory(Path.GetDirectoryName(receiptPath)!);
            await File.WriteAllBytesAsync(receiptPath, receiptPdf);
            
            payment.ReceiptFilePath = receiptPath;
            await _context.SaveChangesAsync();

            // Commit de la transaction
            await transaction.CommitAsync();

            // Envoyer le reçu par email (asynchrone, ne pas attendre)
            if (apartment.PrimaryOwner != null)
            {
                _ = Task.Run(async () =>
                {
                    await _emailService.SendPaymentReceiptEmailAsync(
                        apartment.PrimaryOwner.Email,
                        apartment.PrimaryOwner.FirstName,
                        payment.Amount,
                        receiptPath
                    );
                });

                // Créer une notification
                await _notificationService.CreateNotificationAsync(
                    apartment.PrimaryOwner.Id,
                    "Paiement enregistré",
                    $"Votre paiement de {payment.Amount:C} a été enregistré avec succès.",
                    "Payment",
                    "Payment",
                    payment.Id
                );
            }

            return (true, payment.Id, "Paiement enregistré avec succès.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, 0, $"Erreur lors de l'enregistrement du paiement: {ex.Message}");
        }
    }

    public async Task<PaymentDetailDto?> GetPaymentByIdAsync(int paymentId)
    {
        var payment = await _context.Payments
            .Include(p => p.Apartment)
                .ThenInclude(a => a.Building)
            .Include(p => p.RecordedBy)
            .Include(p => p.PaymentAllocations)
                .ThenInclude(pa => pa.CallForFunds)
                    .ThenInclude(c => c.Charge)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null) return null;

        return new PaymentDetailDto
        {
            Id = payment.Id,
            ApartmentId = payment.ApartmentId,
            BuildingNumber = payment.Apartment.Building.BuildingNumber,
            ApartmentNumber = payment.Apartment.ApartmentNumber,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            ReferenceNumber = payment.ReferenceNumber,
            PaymentDate = payment.PaymentDate,
            RecordedAt = payment.RecordedAt,
            RecordedByName = $"{payment.RecordedBy.FirstName} {payment.RecordedBy.LastName}",
            Notes = payment.Notes,
            ReceiptFilePath = payment.ReceiptFilePath,
            Allocations = payment.PaymentAllocations.Select(pa => new PaymentAllocationDto
            {
                ChargeName = pa.CallForFunds.Charge.Name,
                AllocatedAmount = pa.AllocatedAmount,
                AllocatedAt = pa.AllocatedAt
            }).ToList()
        };
    }

    public async Task<List<PaymentDetailDto>> GetPaymentsByApartmentAsync(int apartmentId, int page = 1, int pageSize = 10)
    {
        var payments = await _context.Payments
            .Include(p => p.Apartment)
                .ThenInclude(a => a.Building)
            .Include(p => p.RecordedBy)
            .Include(p => p.PaymentAllocations)
                .ThenInclude(pa => pa.CallForFunds)
                    .ThenInclude(c => c.Charge)
            .Where(p => p.ApartmentId == apartmentId)
            .OrderByDescending(p => p.PaymentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return payments.Select(p => new PaymentDetailDto
        {
            Id = p.Id,
            ApartmentId = p.ApartmentId,
            BuildingNumber = p.Apartment.Building.BuildingNumber,
            ApartmentNumber = p.Apartment.ApartmentNumber,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod,
            ReferenceNumber = p.ReferenceNumber,
            PaymentDate = p.PaymentDate,
            RecordedAt = p.RecordedAt,
            RecordedByName = $"{p.RecordedBy.FirstName} {p.RecordedBy.LastName}",
            Notes = p.Notes,
            ReceiptFilePath = p.ReceiptFilePath,
            Allocations = p.PaymentAllocations.Select(pa => new PaymentAllocationDto
            {
                ChargeName = pa.CallForFunds.Charge.Name,
                AllocatedAmount = pa.AllocatedAmount,
                AllocatedAt = pa.AllocatedAt
            }).ToList()
        }).ToList();
    }

    public async Task<ApartmentBalanceDto> GetApartmentBalanceAsync(int apartmentId)
    {
        var apartment = await _context.Apartments
            .Include(a => a.Building)
            .FirstOrDefaultAsync(a => a.Id == apartmentId);

        if (apartment == null)
        {
            throw new ArgumentException("Appartement introuvable.");
        }

        // Calculer les totaux
        var totalDue = await _context.CallsForFunds
            .Where(c => c.ApartmentId == apartmentId)
            .SumAsync(c => c.AmountDue);

        var totalPaid = await _context.Payments
            .Where(p => p.ApartmentId == apartmentId)
            .SumAsync(p => p.Amount);

        // Appels de fonds en attente
        var pendingCalls = await _context.CallsForFunds
            .Include(c => c.Charge)
            .Where(c => c.ApartmentId == apartmentId && c.AmountRemaining > 0)
            .OrderBy(c => c.DueDate)
            .Select(c => new CallForFundsDto
            {
                Id = c.Id,
                ChargeName = c.Charge.Name,
                AmountDue = c.AmountDue,
                AmountPaid = c.AmountPaid,
                AmountRemaining = c.AmountRemaining,
                DueDate = c.DueDate,
                Status = c.Status
            })
            .ToListAsync();

        // Paiements récents
        var recentPayments = await GetPaymentsByApartmentAsync(apartmentId, 1, 5);

        return new ApartmentBalanceDto
        {
            ApartmentId = apartmentId,
            BuildingNumber = apartment.Building.BuildingNumber,
            ApartmentNumber = apartment.ApartmentNumber,
            TotalDue = totalDue,
            TotalPaid = totalPaid,
            Balance = totalDue - totalPaid,
            PendingCalls = pendingCalls,
            RecentPayments = recentPayments
        };
    }

    public async Task<byte[]> GeneratePaymentReceiptPdfAsync(int paymentId)
    {
        return await _pdfService.GeneratePaymentReceiptAsync(paymentId);
    }

    /// <summary>
    /// Récupère l'état global des paiements (premier impayé / dernier payé)
    /// </summary>
    public async Task<ApartmentPaymentStatusDto> GetPaymentStatusAsync(int apartmentId)
    {
        var status = new ApartmentPaymentStatusDto
        {
            ApartmentId = apartmentId
        };

        // Récupérer tous les paiements de l'appartement, triés par année/mois
        var allPayments = await _context.MonthlyPayments
            .Where(mp => mp.ApartmentId == apartmentId)
            .OrderByDescending(mp => mp.Year)
            .ThenByDescending(mp => mp.Month)
            .ToListAsync();

        if (!allPayments.Any())
        {
            // Aucun paiement : le premier impayé est janvier de l'année minimale (2022)
            status.FirstUnpaidYear = 2022;
            status.FirstUnpaidMonth = 1;
            return status;
        }

        // Dernier mois payé
        var lastPayment = allPayments.First();
        status.LastPaidYear = lastPayment.Year;
        status.LastPaidMonth = lastPayment.Month;

        // Trouver le premier impayé en parcourant depuis 2022 jusqu'à maintenant
        var currentDate = DateTime.UtcNow;
        var currentYear = currentDate.Year;
        var currentMonth = currentDate.Month;

        for (int year = 2022; year <= currentYear; year++)
        {
            int maxMonth = (year == currentYear) ? currentMonth : 12;
            
            for (int month = 1; month <= maxMonth; month++)
            {
                bool isPaid = allPayments.Any(p => p.Year == year && p.Month == month);
                
                if (!isPaid)
                {
                    status.FirstUnpaidYear = year;
                    status.FirstUnpaidMonth = month;
                    return status;
                }
            }
        }

        // Tous les mois jusqu'à maintenant sont payés
        // Le prochain impayé est le mois suivant le mois actuel
        if (currentMonth == 12)
        {
            status.FirstUnpaidYear = currentYear + 1;
            status.FirstUnpaidMonth = 1;
        }
        else
        {
            status.FirstUnpaidYear = currentYear;
            status.FirstUnpaidMonth = currentMonth + 1;
        }

        return status;
    }

    /// <summary>
    /// Valide que le paiement respecte l'ordre chronologique
    /// </summary>
    public async Task<string?> ValidateChronologicalPaymentAsync(CreateMonthlyPaymentDto dto)
    {
        var status = await GetPaymentStatusAsync(dto.ApartmentId);

        if (!status.FirstUnpaidYear.HasValue)
        {
            return "Tous les paiements sont à jour.";
        }

        // Vérifier que l'année demandée correspond à l'année du premier impayé
        if (dto.Year != status.FirstUnpaidYear.Value)
        {
            return $"Vous devez d'abord payer les mois de l'année {status.FirstUnpaidYear}. Premier mois impayé : {GetMonthName(status.FirstUnpaidMonth!.Value)} {status.FirstUnpaidYear}.";
        }

        // Vérifier que les mois sont consécutifs à partir du premier impayé
        var sortedMonths = dto.Months.OrderBy(m => m).ToList();
        
        if (sortedMonths.First() != status.FirstUnpaidMonth!.Value)
        {
            return $"Vous devez commencer par payer {GetMonthName(status.FirstUnpaidMonth.Value)} {status.FirstUnpaidYear}.";
        }

        // Vérifier la continuité des mois
        for (int i = 0; i < sortedMonths.Count - 1; i++)
        {
            if (sortedMonths[i + 1] != sortedMonths[i] + 1)
            {
                return "Les mois doivent être payés dans l'ordre, sans sauter de mois.";
            }
        }

        return null; // Validation OK
    }

    /// <summary>
    /// Récupère la liste des mois payés pour un appartement et une année
    /// </summary>
    public async Task<List<int>> GetPaidMonthsAsync(int apartmentId, int year)
    {
        return await _context.MonthlyPayments
            .Where(mp => mp.ApartmentId == apartmentId && mp.Year == year && mp.IsPaid)
            .Select(mp => mp.Month)
            .OrderBy(m => m)
            .ToListAsync();
    }

    /// <summary>
    /// Crée des paiements mensuels pour plusieurs mois
    /// </summary>
    public async Task CreateMonthlyPaymentsAsync(CreateMonthlyPaymentDto dto, int recordedByUserId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Vérifier que l'appartement existe
            var apartment = await _context.Apartments
                .Include(a => a.Building)
                .FirstOrDefaultAsync(a => a.Id == dto.ApartmentId);

            if (apartment == null)
            {
                throw new InvalidOperationException("Appartement introuvable.");
            }

            // Récupérer tous les paiements existants pour ces mois (payés ou annulés)
            var existingPayments = await _context.MonthlyPayments
                .Where(mp => mp.ApartmentId == dto.ApartmentId 
                    && mp.Year == dto.Year 
                    && dto.Months.Contains(mp.Month))
                .ToListAsync();

            // Vérifier que les mois ne sont pas déjà actifs
            var activePaidMonths = existingPayments
                .Where(mp => mp.IsPaid)
                .Select(mp => mp.Month)
                .ToList();

            if (activePaidMonths.Any())
            {
                var monthsStr = string.Join(", ", activePaidMonths);
                throw new InvalidOperationException($"Les mois suivants sont déjà payés : {monthsStr}");
            }

            // Montant mensuel fixe (à configurer selon vos besoins)
            const decimal monthlyAmount = 100.00m; // 100 DH par mois

            var now = DateTime.UtcNow;
            var payments = new List<MonthlyPayment>();

            foreach (var month in dto.Months)
            {
                if (month < 1 || month > 12)
                {
                    throw new InvalidOperationException($"Mois invalide : {month}");
                }

                // Vérifier si un paiement existe déjà (annulé)
                var existingPayment = existingPayments.FirstOrDefault(p => p.Month == month);
                
                if (existingPayment != null)
                {
                    // Réactiver le paiement existant
                    existingPayment.IsPaid = true;
                    existingPayment.PaymentDate = now;
                    existingPayment.RecordedById = recordedByUserId;
                    existingPayment.RecordedAt = now;
                    existingPayment.Notes = $"Paiement réactivé - {GetMonthName(month)} {dto.Year}";
                }
                else
                {
                    // Créer un nouveau paiement
                    var payment = new MonthlyPayment
                    {
                        ApartmentId = dto.ApartmentId,
                        Year = dto.Year,
                        Month = month,
                        Amount = monthlyAmount,
                        PaymentDate = now,
                        ReferenceNumber = $"MP-{dto.ApartmentId}-{dto.Year}{month:D2}-{DateTime.Now.Ticks}",
                        RecordedById = recordedByUserId,
                        RecordedAt = now,
                        IsPaid = true,
                        Notes = $"Paiement mensuel - {GetMonthName(month)} {dto.Year}"
                    };

                    payments.Add(payment);
                }
            }

            if (payments.Any())
            {
                await _context.MonthlyPayments.AddRangeAsync(payments);
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Annule (décoche) des paiements mensuels en mettant IsPaid à false
    /// </summary>
    public async Task CancelMonthlyPaymentsAsync(int apartmentId, int year, List<int> months, int canceledByUserId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var paymentsToCancel = await _context.MonthlyPayments
                .Where(mp => mp.ApartmentId == apartmentId 
                    && mp.Year == year 
                    && months.Contains(mp.Month)
                    && mp.IsPaid)
                .ToListAsync();

            foreach (var payment in paymentsToCancel)
            {
                payment.IsPaid = false;
                payment.Notes += $" | Annulé le {DateTime.UtcNow:dd/MM/yyyy} par utilisateur {canceledByUserId}";
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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
            _ => "Inconnu"
        };
    }

    /// <summary>
    /// Obtient le résumé des paiements de tous les appartements pour une année donnée (optimisé)
    /// </summary>
    public async Task<PaymentsSummaryDto> GetPaymentsSummaryByYearAsync(int year)
    {
        var summary = await _context.MonthlyPayments
            .Where(mp => mp.Year == year)
            .GroupBy(mp => mp.ApartmentId)
            .Select(g => new ApartmentPaymentSummaryDto
            {
                ApartmentId = g.Key,
                PaidMonthsCount = g.Count()
            })
            .ToListAsync();

        return new PaymentsSummaryDto
        {
            Year = year,
            Apartments = summary
        };
    }

    /// <summary>
    /// Obtient le résumé des paiements de tous les immeubles pour une année donnée (optimisé)
    /// </summary>
    public async Task<BuildingsPaymentsSummaryDto> GetBuildingsPaymentsSummaryByYearAsync(int year)
    {
        var summary = await _context.MonthlyPayments
            .Where(mp => mp.Year == year)
            .Join(
                _context.Apartments,
                mp => mp.ApartmentId,
                a => a.Id,
                (mp, a) => new { mp, a.BuildingId }
            )
            .GroupBy(x => x.BuildingId)
            .Select(g => new
            {
                BuildingId = g.Key,
                TotalPaidMonths = g.Count()
            })
            .ToListAsync();

        // Récupérer le nombre d'appartements par immeuble
        var buildingsWithCounts = await _context.Buildings
            .Select(b => new
            {
                b.Id,
                ApartmentsCount = _context.Apartments.Count(a => a.BuildingId == b.Id)
            })
            .ToListAsync();

        var result = buildingsWithCounts.Select(b => new BuildingPaymentSummaryDto
        {
            BuildingId = b.Id,
            ApartmentsCount = b.ApartmentsCount,
            TotalPaidMonths = summary.FirstOrDefault(s => s.BuildingId == b.Id)?.TotalPaidMonths ?? 0
        }).ToList();

        return new BuildingsPaymentsSummaryDto
        {
            Year = year,
            Buildings = result
        };
    }
}
