-- =============================================
-- Script de création de la base de données
-- Application de Gestion Syndicale
-- Version 1.0
-- =============================================

USE master;
GO

-- Création de la base de données
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'GestionSyndicale')
BEGIN
    CREATE DATABASE GestionSyndicale;
END
GO

USE GestionSyndicale;
GO

-- =============================================
-- 1. STRUCTURE DE LA RÉSIDENCE
-- =============================================

-- Table Residence: une seule résidence par instance
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Residences]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Residences] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Name] NVARCHAR(200) NOT NULL,
        [Address] NVARCHAR(500) NOT NULL,
        [City] NVARCHAR(100) NOT NULL,
        [PostalCode] NVARCHAR(10) NOT NULL,
        [Phone] NVARCHAR(20) NULL,
        [Email] NVARCHAR(100) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1
    );
END
GO

-- Table Buildings: immeubles de la résidence
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Buildings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Buildings] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [ResidenceId] INT NOT NULL,
        [BuildingNumber] NVARCHAR(10) NOT NULL,
        [Name] NVARCHAR(100) NULL,
        [FloorCount] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [FK_Buildings_Residences] FOREIGN KEY ([ResidenceId]) 
            REFERENCES [dbo].[Residences]([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_Buildings_Number] UNIQUE ([ResidenceId], [BuildingNumber])
    );

    CREATE INDEX [IX_Buildings_ResidenceId] ON [dbo].[Buildings]([ResidenceId]);
END
GO

-- Table Apartments: appartements/lots
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Apartments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Apartments] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [BuildingId] INT NOT NULL,
        [ApartmentNumber] NVARCHAR(10) NOT NULL,
        [Floor] INT NOT NULL,
        [Surface] DECIMAL(10,2) NOT NULL, -- En m²
        [SharesCount] INT NOT NULL DEFAULT 0, -- Tantièmes
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [FK_Apartments_Buildings] FOREIGN KEY ([BuildingId]) 
            REFERENCES [dbo].[Buildings]([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_Apartments_Number] UNIQUE ([BuildingId], [ApartmentNumber])
    );

    CREATE INDEX [IX_Apartments_BuildingId] ON [dbo].[Apartments]([BuildingId]);
    CREATE INDEX [IX_Apartments_IsActive] ON [dbo].[Apartments]([IsActive]);
END
GO

-- =============================================
-- 2. UTILISATEURS ET SÉCURITÉ
-- =============================================

-- Table Roles
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Roles] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Name] NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(200) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [UQ_Roles_Name] UNIQUE ([Name])
    );

    -- Insertion des rôles par défaut
    INSERT INTO [dbo].[Roles] ([Name], [Description])
    VALUES 
        ('SuperAdmin', 'Syndic avec accès complet'),
        ('Admin', 'Administrateur avec accès limité'),
        ('Adherent', 'Résident/Adhérent');
END
GO

-- Table Users
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Email] NVARCHAR(100) NOT NULL,
        [PasswordHash] NVARCHAR(500) NOT NULL,
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [Phone] NVARCHAR(20) NOT NULL,
        [ApartmentId] INT NULL, -- NULL pour SuperAdmin/Admin
        [IsEmailConfirmed] BIT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 0, -- Activé après validation OTP
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [LastLoginAt] DATETIME2 NULL,
        CONSTRAINT [FK_Users_Apartments] FOREIGN KEY ([ApartmentId]) 
            REFERENCES [dbo].[Apartments]([Id]) ON DELETE SET NULL,
        CONSTRAINT [UQ_Users_Email] UNIQUE ([Email]),
        CONSTRAINT [UQ_Users_ApartmentId] UNIQUE ([ApartmentId]) -- Un appart = un user
    );

    CREATE INDEX [IX_Users_Email] ON [dbo].[Users]([Email]);
    CREATE INDEX [IX_Users_IsActive] ON [dbo].[Users]([IsActive]);
    CREATE INDEX [IX_Users_ApartmentId] ON [dbo].[Users]([ApartmentId]);
END
GO

-- Table UserRoles (Many-to-Many)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserRoles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserRoles] (
        [UserId] INT NOT NULL,
        [RoleId] INT NOT NULL,
        [AssignedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY ([RoleId]) 
            REFERENCES [dbo].[Roles]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_UserRoles_UserId] ON [dbo].[UserRoles]([UserId]);
    CREATE INDEX [IX_UserRoles_RoleId] ON [dbo].[UserRoles]([RoleId]);
END
GO

-- Table OtpCodes: codes OTP pour validation
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OtpCodes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OtpCodes] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [UserId] INT NOT NULL,
        [Code] NVARCHAR(6) NOT NULL, -- 6 chiffres
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ExpiresAt] DATETIME2 NOT NULL, -- 15 minutes après création
        [IsUsed] BIT NOT NULL DEFAULT 0,
        [UsedAt] DATETIME2 NULL,
        [Purpose] NVARCHAR(50) NOT NULL, -- Registration, PasswordReset
        CONSTRAINT [FK_OtpCodes_Users] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_OtpCodes_UserId_Code] ON [dbo].[OtpCodes]([UserId], [Code], [ExpiresAt]);
    CREATE INDEX [IX_OtpCodes_ExpiresAt] ON [dbo].[OtpCodes]([ExpiresAt]);
END
GO

-- Table ApartmentComments: commentaires internes (SuperAdmin/Admin uniquement)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApartmentComments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ApartmentComments] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [ApartmentId] INT NOT NULL,
        [CreatedByUserId] INT NOT NULL,
        [Comment] NVARCHAR(MAX) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_ApartmentComments_Apartments] FOREIGN KEY ([ApartmentId]) 
            REFERENCES [dbo].[Apartments]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ApartmentComments_Users] FOREIGN KEY ([CreatedByUserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_ApartmentComments_ApartmentId] ON [dbo].[ApartmentComments]([ApartmentId]);
    CREATE INDEX [IX_ApartmentComments_CreatedAt] ON [dbo].[ApartmentComments]([CreatedAt] DESC);
END
GO

-- =============================================
-- 3. GESTION FINANCIÈRE
-- =============================================

-- Table ExpenseCategories
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExpenseCategories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ExpenseCategories] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [UQ_ExpenseCategories_Name] UNIQUE ([Name])
    );

    -- Catégories par défaut
    INSERT INTO [dbo].[ExpenseCategories] ([Name], [Description])
    VALUES 
        ('Entretien', 'Entretien courant de la résidence'),
        ('Réparations', 'Réparations et travaux'),
        ('Assurance', 'Assurances de la copropriété'),
        ('Eau', 'Consommation d''eau'),
        ('Électricité', 'Consommation électrique parties communes'),
        ('Jardinage', 'Entretien espaces verts'),
        ('Nettoyage', 'Nettoyage parties communes'),
        ('Administratif', 'Frais administratifs et honoraires');
END
GO

-- Table Suppliers
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Suppliers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Suppliers] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Name] NVARCHAR(200) NOT NULL,
        [Phone] NVARCHAR(20) NULL,
        [Email] NVARCHAR(100) NULL,
        [Address] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX [IX_Suppliers_Name] ON [dbo].[Suppliers]([Name]);
END
GO

-- Table Charges
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Charges]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Charges] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Name] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL, -- Montant total résidence
        [ChargeType] NVARCHAR(50) NOT NULL, -- Monthly, Annual, Exceptional
        [EffectiveDate] DATE NOT NULL,
        [EndDate] DATE NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedByUserId] INT NOT NULL,
        CONSTRAINT [FK_Charges_Users] FOREIGN KEY ([CreatedByUserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_Charges_EffectiveDate] ON [dbo].[Charges]([EffectiveDate]);
    CREATE INDEX [IX_Charges_ChargeType] ON [dbo].[Charges]([ChargeType]);
END
GO

-- Table CallsForFunds: appels de fonds par appartement
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CallsForFunds]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CallsForFunds] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [ChargeId] INT NOT NULL,
        [ApartmentId] INT NOT NULL,
        [AmountDue] DECIMAL(18,2) NOT NULL, -- Calculé selon tantièmes
        [AmountPaid] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [AmountRemaining] DECIMAL(18,2) NOT NULL,
        [DueDate] DATE NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, PartiallyPaid, Paid, Overdue
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [FK_CallsForFunds_Charges] FOREIGN KEY ([ChargeId]) 
            REFERENCES [dbo].[Charges]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CallsForFunds_Apartments] FOREIGN KEY ([ApartmentId]) 
            REFERENCES [dbo].[Apartments]([Id]) ON DELETE CASCADE,
        CONSTRAINT [CHK_CallsForFunds_AmountPaid] CHECK ([AmountPaid] >= 0),
        CONSTRAINT [CHK_CallsForFunds_AmountRemaining] CHECK ([AmountRemaining] >= 0)
    );

    CREATE INDEX [IX_CallsForFunds_ApartmentId_DueDate] ON [dbo].[CallsForFunds]([ApartmentId], [DueDate]);
    CREATE INDEX [IX_CallsForFunds_Status] ON [dbo].[CallsForFunds]([Status]);
    CREATE INDEX [IX_CallsForFunds_DueDate] ON [dbo].[CallsForFunds]([DueDate]);
END
GO

-- Table Payments: historique des paiements (IMMUABLE)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Payments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Payments] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [ApartmentId] INT NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [PaymentMethod] NVARCHAR(50) NOT NULL, -- Cash, Check, Transfer, Card
        [ReferenceNumber] NVARCHAR(100) NULL, -- Numéro chèque ou référence
        [PaymentDate] DATE NOT NULL,
        [RecordedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [RecordedByUserId] INT NOT NULL,
        [Notes] NVARCHAR(500) NULL,
        [ReceiptFilePath] NVARCHAR(500) NULL, -- Chemin du PDF
        CONSTRAINT [FK_Payments_Apartments] FOREIGN KEY ([ApartmentId]) 
            REFERENCES [dbo].[Apartments]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Payments_Users] FOREIGN KEY ([RecordedByUserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [CHK_Payments_Amount] CHECK ([Amount] > 0)
    );

    CREATE INDEX [IX_Payments_ApartmentId_PaymentDate] ON [dbo].[Payments]([ApartmentId], [PaymentDate] DESC);
    CREATE INDEX [IX_Payments_PaymentDate] ON [dbo].[Payments]([PaymentDate] DESC);
    CREATE INDEX [IX_Payments_RecordedAt] ON [dbo].[Payments]([RecordedAt] DESC);
END
GO

-- Table PaymentAllocations: allocation paiements aux appels de fonds
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentAllocations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PaymentAllocations] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [PaymentId] INT NOT NULL,
        [CallForFundsId] INT NOT NULL,
        [AllocatedAmount] DECIMAL(18,2) NOT NULL,
        [AllocatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_PaymentAllocations_Payments] FOREIGN KEY ([PaymentId]) 
            REFERENCES [dbo].[Payments]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PaymentAllocations_CallsForFunds] FOREIGN KEY ([CallForFundsId]) 
            REFERENCES [dbo].[CallsForFunds]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [CHK_PaymentAllocations_Amount] CHECK ([AllocatedAmount] > 0)
    );

    CREATE INDEX [IX_PaymentAllocations_PaymentId] ON [dbo].[PaymentAllocations]([PaymentId]);
    CREATE INDEX [IX_PaymentAllocations_CallForFundsId] ON [dbo].[PaymentAllocations]([CallForFundsId]);
END
GO

-- Table Expenses
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Expenses]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Expenses] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [CategoryId] INT NOT NULL,
        [SupplierId] INT NULL,
        [Description] NVARCHAR(500) NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [ExpenseDate] DATE NOT NULL,
        [RecordedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [RecordedByUserId] INT NOT NULL,
        [InvoiceNumber] NVARCHAR(100) NULL,
        [Notes] NVARCHAR(500) NULL,
        CONSTRAINT [FK_Expenses_Categories] FOREIGN KEY ([CategoryId]) 
            REFERENCES [dbo].[ExpenseCategories]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Expenses_Suppliers] FOREIGN KEY ([SupplierId]) 
            REFERENCES [dbo].[Suppliers]([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Expenses_Users] FOREIGN KEY ([RecordedByUserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [CHK_Expenses_Amount] CHECK ([Amount] > 0)
    );

    CREATE INDEX [IX_Expenses_ExpenseDate] ON [dbo].[Expenses]([ExpenseDate] DESC);
    CREATE INDEX [IX_Expenses_CategoryId] ON [dbo].[Expenses]([CategoryId]);
    CREATE INDEX [IX_Expenses_RecordedAt] ON [dbo].[Expenses]([RecordedAt] DESC);
END
GO

-- Table ExpenseAttachments: pièces justificatives
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExpenseAttachments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ExpenseAttachments] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [ExpenseId] INT NOT NULL,
        [FileName] NVARCHAR(255) NOT NULL,
        [FilePath] NVARCHAR(500) NOT NULL,
        [FileType] NVARCHAR(100) NOT NULL, -- MIME type
        [FileSize] BIGINT NOT NULL, -- En bytes
        [UploadedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UploadedByUserId] INT NOT NULL,
        CONSTRAINT [FK_ExpenseAttachments_Expenses] FOREIGN KEY ([ExpenseId]) 
            REFERENCES [dbo].[Expenses]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ExpenseAttachments_Users] FOREIGN KEY ([UploadedByUserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_ExpenseAttachments_ExpenseId] ON [dbo].[ExpenseAttachments]([ExpenseId]);
END
GO

-- =============================================
-- 4. COMMUNICATION
-- =============================================

-- Table Notifications
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Notifications] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [UserId] INT NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Message] NVARCHAR(MAX) NOT NULL,
        [Type] NVARCHAR(50) NOT NULL, -- Info, Warning, Payment, News
        [IsRead] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ReadAt] DATETIME2 NULL,
        [RelatedEntityType] NVARCHAR(50) NULL, -- Payment, NewsPost, etc.
        [RelatedEntityId] INT NULL,
        CONSTRAINT [FK_Notifications_Users] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_Notifications_UserId_IsRead] ON [dbo].[Notifications]([UserId], [IsRead], [CreatedAt] DESC);
    CREATE INDEX [IX_Notifications_CreatedAt] ON [dbo].[Notifications]([CreatedAt] DESC);
END
GO

-- Table NewsPosts
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NewsPosts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[NewsPosts] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Title] NVARCHAR(200) NOT NULL,
        [Content] NVARCHAR(MAX) NOT NULL,
        [PublishedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [PublishedByUserId] INT NOT NULL,
        [IsPublished] BIT NOT NULL DEFAULT 1,
        [UpdatedAt] DATETIME2 NULL,
        [Category] NVARCHAR(50) NULL, -- Général, Travaux, Réunion
        CONSTRAINT [FK_NewsPosts_Users] FOREIGN KEY ([PublishedByUserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_NewsPosts_PublishedAt] ON [dbo].[NewsPosts]([PublishedAt] DESC);
    CREATE INDEX [IX_NewsPosts_Category] ON [dbo].[NewsPosts]([Category]);
END
GO

-- Table NewsAttachments
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NewsAttachments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[NewsAttachments] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [NewsPostId] INT NOT NULL,
        [FileName] NVARCHAR(255) NOT NULL,
        [FilePath] NVARCHAR(500) NOT NULL,
        [FileType] NVARCHAR(100) NOT NULL,
        [FileSize] BIGINT NOT NULL,
        [UploadedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_NewsAttachments_NewsPosts] FOREIGN KEY ([NewsPostId]) 
            REFERENCES [dbo].[NewsPosts]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_NewsAttachments_NewsPostId] ON [dbo].[NewsAttachments]([NewsPostId]);
END
GO

-- Table Documents
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Documents] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [FileName] NVARCHAR(255) NOT NULL,
        [FilePath] NVARCHAR(500) NOT NULL,
        [FileType] NVARCHAR(100) NOT NULL,
        [FileSize] BIGINT NOT NULL,
        [Category] NVARCHAR(50) NOT NULL, -- Règlement, PV, Technique
        [UploadedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UploadedByUserId] INT NOT NULL,
        [IsPublic] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [FK_Documents_Users] FOREIGN KEY ([UploadedByUserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_Documents_Category] ON [dbo].[Documents]([Category]);
    CREATE INDEX [IX_Documents_UploadedAt] ON [dbo].[Documents]([UploadedAt] DESC);
END
GO

-- =============================================
-- 5. AUDIT ET RAPPORTS
-- =============================================

-- Table AuditLogs
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [UserId] INT NULL, -- NULL pour actions système
        [Action] NVARCHAR(100) NOT NULL, -- Create, Update, Delete, Login
        [EntityType] NVARCHAR(100) NOT NULL, -- Payment, User, Expense
        [EntityId] INT NULL,
        [OldValues] NVARCHAR(MAX) NULL, -- JSON
        [NewValues] NVARCHAR(MAX) NULL, -- JSON
        [IpAddress] NVARCHAR(50) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_AuditLogs_Users] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE SET NULL
    );

    CREATE INDEX [IX_AuditLogs_CreatedAt] ON [dbo].[AuditLogs]([CreatedAt] DESC);
    CREATE INDEX [IX_AuditLogs_EntityType_EntityId] ON [dbo].[AuditLogs]([EntityType], [EntityId]);
    CREATE INDEX [IX_AuditLogs_UserId] ON [dbo].[AuditLogs]([UserId]);
END
GO

-- Table MonthlyReports
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MonthlyReports]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MonthlyReports] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Year] INT NOT NULL,
        [Month] INT NOT NULL,
        [TotalPaymentsReceived] DECIMAL(18,2) NOT NULL,
        [TotalExpenses] DECIMAL(18,2) NOT NULL,
        [Balance] DECIMAL(18,2) NOT NULL,
        [PaymentsCount] INT NOT NULL,
        [ExpensesCount] INT NOT NULL,
        [ReportFilePath] NVARCHAR(500) NULL,
        [GeneratedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [GeneratedByUserId] INT NOT NULL,
        CONSTRAINT [FK_MonthlyReports_Users] FOREIGN KEY ([GeneratedByUserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [UQ_MonthlyReports_Year_Month] UNIQUE ([Year], [Month])
    );

    CREATE INDEX [IX_MonthlyReports_Year_Month] ON [dbo].[MonthlyReports]([Year], [Month]);
END
GO

-- Table AnnualReports
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AnnualReports]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AnnualReports] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Year] INT NOT NULL,
        [TotalPaymentsReceived] DECIMAL(18,2) NOT NULL,
        [TotalExpenses] DECIMAL(18,2) NOT NULL,
        [Balance] DECIMAL(18,2) NOT NULL,
        [ReportFilePath] NVARCHAR(500) NULL,
        [GeneratedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [GeneratedByUserId] INT NOT NULL,
        CONSTRAINT [FK_AnnualReports_Users] FOREIGN KEY ([GeneratedByUserId]) 
            REFERENCES [dbo].[Users]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [UQ_AnnualReports_Year] UNIQUE ([Year])
    );

    CREATE INDEX [IX_AnnualReports_Year] ON [dbo].[AnnualReports]([Year]);
END
GO

PRINT 'Base de données créée avec succès!';
GO
