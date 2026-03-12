-- Migration: Ajout du Soft Delete aux tables de dépenses
-- Date: 2025-12-25
-- Description: Ajoute les colonnes IsDeleted et DeletedAt pour la traçabilité

USE GestionSyndicale;
GO

-- Table Expenses
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Expenses') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE dbo.Expenses ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Expenses_IsDeleted DEFAULT 0;
    PRINT 'Colonne IsDeleted ajoutée à la table Expenses';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Expenses') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE dbo.Expenses ADD DeletedAt DATETIME2(7) NULL;
    PRINT 'Colonne DeletedAt ajoutée à la table Expenses';
END

-- Table ExpenseCategories
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ExpenseCategories') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE dbo.ExpenseCategories ADD IsDeleted BIT NOT NULL CONSTRAINT DF_ExpenseCategories_IsDeleted DEFAULT 0;
    PRINT 'Colonne IsDeleted ajoutée à la table ExpenseCategories';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ExpenseCategories') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE dbo.ExpenseCategories ADD DeletedAt DATETIME2(7) NULL;
    PRINT 'Colonne DeletedAt ajoutée à la table ExpenseCategories';
END

-- Table ExpenseAttachments
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ExpenseAttachments') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE dbo.ExpenseAttachments ADD IsDeleted BIT NOT NULL CONSTRAINT DF_ExpenseAttachments_IsDeleted DEFAULT 0;
    PRINT 'Colonne IsDeleted ajoutée à la table ExpenseAttachments';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ExpenseAttachments') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE dbo.ExpenseAttachments ADD DeletedAt DATETIME2(7) NULL;
    PRINT 'Colonne DeletedAt ajoutée à la table ExpenseAttachments';
END
GO

-- Index pour améliorer les performances des requêtes avec IsDeleted
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Expenses_IsDeleted' AND object_id = OBJECT_ID(N'dbo.Expenses'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Expenses_IsDeleted 
    ON dbo.Expenses(IsDeleted) 
    WHERE IsDeleted = 0;
    PRINT 'Index IX_Expenses_IsDeleted créé';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExpenseCategories_IsDeleted' AND object_id = OBJECT_ID(N'dbo.ExpenseCategories'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExpenseCategories_IsDeleted 
    ON dbo.ExpenseCategories(IsDeleted) 
    WHERE IsDeleted = 0;
    PRINT 'Index IX_ExpenseCategories_IsDeleted créé';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExpenseAttachments_IsDeleted' AND object_id = OBJECT_ID(N'dbo.ExpenseAttachments'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExpenseAttachments_IsDeleted 
    ON dbo.ExpenseAttachments(IsDeleted) 
    WHERE IsDeleted = 0;
    PRINT 'Index IX_ExpenseAttachments_IsDeleted créé';
END

PRINT 'Migration terminée avec succès!';
GO
