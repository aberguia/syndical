-- Script pour ajouter les colonnes IsDeleted aux tables Users et Apartments
-- Date: 2025-12-23
-- Description: Ajout du support du soft delete

USE GestionSyndicale;
GO

-- Ajouter IsDeleted à la table Users
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD IsDeleted BIT NOT NULL DEFAULT 0;
    
    PRINT 'Colonne IsDeleted ajoutée à la table Users';
END
ELSE
BEGIN
    PRINT 'Colonne IsDeleted existe déjà dans la table Users';
END
GO

-- Ajouter IsDeleted à la table Apartments
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Apartments]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [dbo].[Apartments]
    ADD IsDeleted BIT NOT NULL DEFAULT 0;
    
    PRINT 'Colonne IsDeleted ajoutée à la table Apartments';
END
ELSE
BEGIN
    PRINT 'Colonne IsDeleted existe déjà dans la table Apartments';
END
GO

PRINT 'Script d''ajout des colonnes IsDeleted terminé avec succès';
GO
