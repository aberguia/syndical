-- =============================================
-- Création de la table MonthlyPayments
-- Date: 22/12/2025
-- Description: Table pour stocker les paiements mensuels de cotisation
-- =============================================

USE GestionSyndicale;
GO

-- Créer la table MonthlyPayments
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MonthlyPayments')
BEGIN
    CREATE TABLE MonthlyPayments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ApartmentId INT NOT NULL,
        [Year] INT NOT NULL,
        [Month] INT NOT NULL CHECK ([Month] BETWEEN 1 AND 12),
        Amount DECIMAL(18,2) NOT NULL,
        PaymentDate DATETIME2 NOT NULL,
        ReferenceNumber NVARCHAR(100),
        RecordedById INT NOT NULL,
        RecordedAt DATETIME2 NOT NULL,
        Notes NVARCHAR(500),
        
        -- Foreign Keys
        CONSTRAINT FK_MonthlyPayments_Apartment FOREIGN KEY (ApartmentId) 
            REFERENCES Apartments(Id) ON DELETE CASCADE,
        CONSTRAINT FK_MonthlyPayments_User FOREIGN KEY (RecordedById) 
            REFERENCES Users(Id),
        
        -- Contrainte unique : un appartement ne peut payer qu'une fois par mois/année
        CONSTRAINT UQ_MonthlyPayment_ApartmentYearMonth UNIQUE (ApartmentId, [Year], [Month])
    );

    -- Index pour recherche rapide
    CREATE INDEX IX_MonthlyPayments_Apartment_Year ON MonthlyPayments(ApartmentId, [Year]);
    CREATE INDEX IX_MonthlyPayments_PaymentDate ON MonthlyPayments(PaymentDate);

    PRINT 'Table MonthlyPayments créée avec succès.';
END
ELSE
BEGIN
    PRINT 'Table MonthlyPayments existe déjà.';
END
GO
