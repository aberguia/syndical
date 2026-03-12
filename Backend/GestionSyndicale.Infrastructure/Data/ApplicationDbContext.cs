using GestionSyndicale.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionSyndicale.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Structure
    public DbSet<Residence> Residences { get; set; }
    public DbSet<Building> Buildings { get; set; }
    public DbSet<Apartment> Apartments { get; set; }

    // Utilisateurs
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<OtpCode> OtpCodes { get; set; }
    public DbSet<ApartmentComment> ApartmentComments { get; set; }

    // Finances
    public DbSet<Charge> Charges { get; set; }
    public DbSet<CallForFunds> CallsForFunds { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentAllocation> PaymentAllocations { get; set; }
    public DbSet<MonthlyPayment> MonthlyPayments { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<ExpenseAttachment> ExpenseAttachments { get; set; }
    public DbSet<OtherRevenue> OtherRevenues { get; set; }

    // Communication
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NewsPost> NewsPosts { get; set; }
    public DbSet<NewsAttachment> NewsAttachments { get; set; }
    public DbSet<Document> Documents { get; set; }

    // Announcements & Polls
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<Poll> Polls { get; set; }
    public DbSet<PollOption> PollOptions { get; set; }
    public DbSet<PollVote> PollVotes { get; set; }

    // Audit & Reports
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<MonthlyReport> MonthlyReports { get; set; }
    public DbSet<AnnualReport> AnnualReports { get; set; }

    // Parking
    public DbSet<Car> Cars { get; set; }
    public DbSet<ParkingStatus> ParkingStatuses { get; set; }

    // Notes
    public DbSet<MemberNote> MemberNotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration User - Apartment (1 appartement = 1 user max)
        modelBuilder.Entity<User>()
            .HasOne(u => u.Apartment)
            .WithOne(a => a.PrimaryOwner)
            .HasForeignKey<User>(u => u.ApartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configuration UserRole (clé composite)
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        // Index pour performance
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Apartment>()
            .HasIndex(a => new { a.BuildingId, a.ApartmentNumber })
            .IsUnique();

        modelBuilder.Entity<Payment>()
            .HasIndex(p => new { p.ApartmentId, p.PaymentDate });

        modelBuilder.Entity<Expense>()
            .HasIndex(e => e.ExpenseDate);

        // Configuration de la relation Expense -> User (RecordedBy)
        modelBuilder.Entity<Expense>()
            .HasOne(e => e.RecordedBy)
            .WithMany()
            .HasForeignKey(e => e.RecordedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configuration de la relation ExpenseAttachment -> User (UploadedBy)
        modelBuilder.Entity<ExpenseAttachment>()
            .HasOne(a => a.UploadedBy)
            .WithMany()
            .HasForeignKey(a => a.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CallForFunds>()
            .HasIndex(c => new { c.ApartmentId, c.DueDate });

        modelBuilder.Entity<OtpCode>()
            .HasIndex(o => new { o.UserId, o.Code, o.ExpiresAt });

        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });

        // Précision décimale pour montants
        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Expense>()
            .Property(e => e.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Charge>()
            .Property(c => c.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CallForFunds>()
            .Property(c => c.AmountDue)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CallForFunds>()
            .Property(c => c.AmountPaid)
            .HasPrecision(18, 2);

        modelBuilder.Entity<CallForFunds>()
            .Property(c => c.AmountRemaining)
            .HasPrecision(18, 2);

        // Configuration MonthlyPayment
        modelBuilder.Entity<MonthlyPayment>()
            .Property(mp => mp.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<MonthlyPayment>()
            .HasIndex(mp => new { mp.ApartmentId, mp.Year, mp.Month })
            .IsUnique(); // Un appartement ne peut payer qu'une fois par mois

        // Configuration Car - Index unique sur plaque
        modelBuilder.Entity<Car>()
            .HasIndex(c => new { c.PlatePart1, c.PlatePart2, c.PlatePart3 })
            .IsUnique();

        // Filtres globaux pour soft delete
        modelBuilder.Entity<Expense>()
            .HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<ExpenseAttachment>()
            .HasQueryFilter(ea => !ea.IsDeleted);

        // Configuration OtherRevenue
        modelBuilder.Entity<OtherRevenue>()
            .Property(or => or.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OtherRevenue>()
            .HasOne(or => or.RecordedBy)
            .WithMany()
            .HasForeignKey(or => or.RecordedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OtherRevenue>()
            .HasIndex(or => or.RevenueDate);

        modelBuilder.Entity<OtherRevenue>()
            .HasQueryFilter(or => !or.IsDeleted);

        // Configuration Document
        modelBuilder.Entity<Document>()
            .HasOne(d => d.UploadedBy)
            .WithMany()
            .HasForeignKey(d => d.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Document>()
            .HasQueryFilter(d => !d.IsDeleted);

        // Configuration Announcement
        modelBuilder.Entity<Announcement>()
            .HasOne(a => a.CreatedBy)
            .WithMany()
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Announcement>()
            .HasOne(a => a.UpdatedBy)
            .WithMany()
            .HasForeignKey(a => a.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Announcement>()
            .HasIndex(a => a.Status);

        modelBuilder.Entity<Announcement>()
            .HasIndex(a => a.CreatedOn);

        // Configuration Poll
        modelBuilder.Entity<Poll>()
            .HasOne(p => p.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Poll>()
            .HasMany(p => p.Options)
            .WithOne(po => po.Poll)
            .HasForeignKey(po => po.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Poll>()
            .HasMany(p => p.Votes)
            .WithOne(pv => pv.Poll)
            .HasForeignKey(pv => pv.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Poll>()
            .HasIndex(p => p.Status);

        modelBuilder.Entity<Poll>()
            .HasIndex(p => p.CreatedOn);

        // Configuration PollOption
        modelBuilder.Entity<PollOption>()
            .HasMany(po => po.Votes)
            .WithOne(pv => pv.PollOption)
            .HasForeignKey(pv => pv.PollOptionId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PollOption>()
            .HasIndex(po => new { po.PollId, po.SortOrder });

        // Configuration PollVote (unique vote per adherent per poll)
        modelBuilder.Entity<PollVote>()
            .HasOne(pv => pv.Adherent)
            .WithMany()
            .HasForeignKey(pv => pv.AdherentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PollVote>()
            .HasIndex(pv => new { pv.PollId, pv.AdherentId })
            .IsUnique();

        modelBuilder.Entity<PollVote>()
            .HasIndex(pv => pv.PollId);

        modelBuilder.Entity<PollVote>()
            .HasIndex(pv => pv.AdherentId);

        // Seed des rôles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "SuperAdmin", Description = "Syndic avec accès complet" },
            new Role { Id = 2, Name = "Admin", Description = "Administrateur avec accès limité" },
            new Role { Id = 3, Name = "Adherent", Description = "Résident/Adhérent" }
        );
    }
}
