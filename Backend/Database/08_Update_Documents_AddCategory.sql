-- Migration: Mise à jour de la table Documents pour supporter les catégories
-- Date: 2025-12-25
-- Description: Ajoute la colonne Category à la table Documents

USE GestionSyndicale;
GO

-- Ajout de la colonne Category à Documents
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Documents') AND name = 'Category')
BEGIN
    ALTER TABLE dbo.Documents ADD Category NVARCHAR(50) NULL;
    PRINT 'Colonne Category ajoutée à Documents';
    
    -- Mettre à jour les documents existants avec une valeur par défaut si nécessaire
    UPDATE dbo.Documents 
    SET Category = 'General' 
    WHERE Category IS NULL;
    
    PRINT 'Valeurs par défaut appliquées';
END
ELSE
BEGIN
    PRINT 'Colonne Category existe déjà dans Documents';
END
GO

-- Créer un index sur Category pour améliorer les performances
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Documents_Category' AND object_id = OBJECT_ID(N'dbo.Documents'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Documents_Category 
    ON dbo.Documents(Category);
    PRINT 'Index IX_Documents_Category créé';
END
GO

PRINT 'Modification de Documents terminée avec succès!';
GO
