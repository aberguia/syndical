-- Migration: Mise à jour des colonnes Documents et création OtherRevenues
-- Date: 2025-12-25
-- Description: Ajoute les colonnes manquantes pour supporter OtherRevenue

USE GestionSyndicale;
GO

-- Mise à jour de la table Documents
-- Ajouter RelatedEntityId si elle n'existe pas
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Documents') AND name = 'RelatedEntityId')
BEGIN
    ALTER TABLE dbo.Documents ADD RelatedEntityId INT NULL;
    PRINT 'Colonne RelatedEntityId ajoutée à Documents';
END
ELSE
BEGIN
    PRINT 'Colonne RelatedEntityId existe déjà dans Documents';
END

-- Ajouter IsDeleted si elle n'existe pas
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Documents') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE dbo.Documents ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Documents_IsDeleted DEFAULT 0;
    PRINT 'Colonne IsDeleted ajoutée à Documents';
END
ELSE
BEGIN
    PRINT 'Colonne IsDeleted existe déjà dans Documents';
END

-- Ajouter DeletedAt si elle n'existe pas
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Documents') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE dbo.Documents ADD DeletedAt DATETIME2(7) NULL;
    PRINT 'Colonne DeletedAt ajoutée à Documents';
END
ELSE
BEGIN
    PRINT 'Colonne DeletedAt existe déjà dans Documents';
END
GO

-- Créer la table OtherRevenues si elle n'existe pas
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OtherRevenues')
BEGIN
    CREATE TABLE dbo.OtherRevenues (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RevenueDate DATETIME2(7) NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        RecordedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
        RecordedByUserId INT NOT NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2(7) NULL,
        
        CONSTRAINT FK_OtherRevenues_Users_RecordedBy FOREIGN KEY (RecordedByUserId)
            REFERENCES Users(Id) ON DELETE NO ACTION
    );

    CREATE NONCLUSTERED INDEX IX_OtherRevenues_RevenueDate 
    ON dbo.OtherRevenues(RevenueDate);

    CREATE NONCLUSTERED INDEX IX_OtherRevenues_IsDeleted 
    ON dbo.OtherRevenues(IsDeleted) 
    WHERE IsDeleted = 0;

    PRINT 'Table OtherRevenues créée avec succès';
END
ELSE
BEGIN
    PRINT 'Table OtherRevenues existe déjà';
END
GO

-- Créer un index sur Documents.IsDeleted si il n'existe pas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Documents_IsDeleted' AND object_id = OBJECT_ID(N'dbo.Documents'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Documents_IsDeleted 
    ON dbo.Documents(IsDeleted) 
    WHERE IsDeleted = 0;
    PRINT 'Index IX_Documents_IsDeleted créé';
END
ELSE
BEGIN
    PRINT 'Index IX_Documents_IsDeleted existe déjà';
END
GO

PRINT 'Migration terminée avec succès!';
GO
