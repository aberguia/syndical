-- Ajout de la colonne IsPaid à la table MonthlyPayments
-- Cette colonne permet de marquer un paiement comme annulé/décoché sans le supprimer (soft delete)

USE GestionSyndicale;
GO

-- Ajouter la colonne IsPaid (par défaut TRUE pour tous les enregistrements existants)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MonthlyPayments]') AND name = 'IsPaid')
BEGIN
    ALTER TABLE [dbo].[MonthlyPayments]
    ADD [IsPaid] BIT NOT NULL DEFAULT 1;
    
    PRINT 'Colonne IsPaid ajoutée à la table MonthlyPayments avec succès';
END
ELSE
BEGIN
    PRINT 'La colonne IsPaid existe déjà dans la table MonthlyPayments';
END
GO
