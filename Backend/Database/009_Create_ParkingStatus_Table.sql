-- Script de migration : Création de la table ParkingStatus
-- Date: 2024-12-24

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Création de la table ParkingStatus
CREATE TABLE ParkingStatuses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CurrentCars INT NOT NULL DEFAULT 0,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT CK_ParkingStatuses_CurrentCars_NonNegative CHECK (CurrentCars >= 0)
);
GO

-- Insérer l'enregistrement par défaut
INSERT INTO ParkingStatuses (CurrentCars, UpdatedAt)
VALUES (0, GETUTCDATE());
GO

PRINT 'Table ParkingStatuses créée avec succès';
