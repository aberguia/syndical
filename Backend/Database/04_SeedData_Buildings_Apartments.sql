-- =============================================
-- Script de remplissage : Immeubles et Appartements
-- Date: 22/12/2025
-- Description: Crée 22 immeubles (76-97) avec 16 appartements chacun
--              3 étages + RDC = 4 niveaux avec 4 appartements par étage
-- =============================================

USE GestionSyndicale;
GO

-- Supprimer les données existantes (ordre important : FK contraintes)
PRINT 'Suppression des appartements existants...';
DELETE FROM Apartments WHERE BuildingId IN (SELECT Id FROM Buildings WHERE BuildingNumber BETWEEN '76' AND '97');
PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' appartements supprimés.';

PRINT 'Suppression des immeubles existants (76-97)...';
DELETE FROM Buildings WHERE BuildingNumber BETWEEN '76' AND '97';
PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' immeubles supprimés.';
GO

-- Variables
DECLARE @ResidenceId INT = 1; -- ID de la résidence (depuis table Residences)
DECLARE @BuildingNumber INT = 76;
DECLARE @BuildingId INT;
DECLARE @ApartmentNumber INT;
DECLARE @Floor INT;
DECLARE @Surface DECIMAL(6,2);
DECLARE @SharesCount INT;

-- =============================================
-- INSERTION DES IMMEUBLES (76 à 97)
-- =============================================
PRINT '---------------------------------------------';
PRINT 'Insertion des immeubles 76 à 97...';
PRINT '---------------------------------------------';

WHILE @BuildingNumber <= 97
BEGIN
    INSERT INTO Buildings (
        ResidenceId,
        BuildingNumber,
        Name,
        FloorCount,
        IsActive,
        CreatedAt,
        UpdatedAt
    )
    VALUES (
        @ResidenceId,
        CAST(@BuildingNumber AS NVARCHAR(10)),
        CAST(@BuildingNumber AS NVARCHAR(100)),
        3, -- 3 étages + RDC = 4 niveaux
        1, -- Actif
        GETUTCDATE(),
        GETUTCDATE()
    );

    SET @BuildingId = SCOPE_IDENTITY();
    PRINT 'Immeuble ' + CAST(@BuildingNumber AS VARCHAR(10)) + ' créé (ID: ' + CAST(@BuildingId AS VARCHAR(10)) + ')';

    -- =============================================
    -- INSERTION DES 16 APPARTEMENTS pour cet immeuble
    -- =============================================
    SET @ApartmentNumber = 1;
    
    WHILE @ApartmentNumber <= 16
    BEGIN
        -- Calcul de l'étage (4 appartements par étage)
        -- Appartements 1-4 = Étage 0 (RDC)
        -- Appartements 5-8 = Étage 1
        -- Appartements 9-12 = Étage 2
        -- Appartements 13-16 = Étage 3
        SET @Floor = (@ApartmentNumber - 1) / 4;
        
        -- Surface aléatoire entre 45m² et 120m²
        SET @Surface = 45 + (ABS(CHECKSUM(NEWID())) % 76); -- 45 + random(0-75)
        
        -- Tantièmes aléatoires entre 80 et 150
        SET @SharesCount = 80 + (ABS(CHECKSUM(NEWID())) % 71); -- 80 + random(0-70)
        
        INSERT INTO Apartments (
            BuildingId,
            ApartmentNumber,
            Floor,
            Surface,
            SharesCount,
            IsActive,
            CreatedAt,
            UpdatedAt
        )
        VALUES (
            @BuildingId,
            CAST(@ApartmentNumber AS NVARCHAR(10)),
            @Floor,
            @Surface,
            @SharesCount,
            1, -- Actif
            GETUTCDATE(),
            GETUTCDATE()
        );
        
        SET @ApartmentNumber = @ApartmentNumber + 1;
    END
    
    PRINT '  -> 16 appartements créés pour immeuble ' + CAST(@BuildingNumber AS VARCHAR(10));
    
    SET @BuildingNumber = @BuildingNumber + 1;
END

-- =============================================
-- STATISTIQUES FINALES
-- =============================================
PRINT '';
PRINT '=============================================';
PRINT 'STATISTIQUES FINALES';
PRINT '=============================================';

DECLARE @TotalBuildings INT;
DECLARE @TotalApartments INT;

SELECT @TotalBuildings = COUNT(*) FROM Buildings;
SELECT @TotalApartments = COUNT(*) FROM Apartments;

PRINT 'Immeubles créés : ' + CAST(@TotalBuildings AS VARCHAR(10));
PRINT 'Appartements créés : ' + CAST(@TotalApartments AS VARCHAR(10));
PRINT 'Moyenne apparts/immeuble : ' + CAST(@TotalApartments / @TotalBuildings AS VARCHAR(10));

-- Détail par immeuble
PRINT '';
PRINT 'Détail par immeuble :';
PRINT '---------------------------------------------';

SELECT 
    b.BuildingNumber AS [Code],
    b.Name AS [Nom],
    b.FloorCount AS [Étages],
    COUNT(a.Id) AS [Nb Apparts],
    AVG(a.Surface) AS [Surface Moy.],
    SUM(a.SharesCount) AS [Total Tantièmes]
FROM Buildings b
LEFT JOIN Apartments a ON b.Id = a.BuildingId
GROUP BY b.Id, b.BuildingNumber, b.Name, b.FloorCount
ORDER BY b.BuildingNumber;

-- Vérification de la contrainte unique (BuildingId + ApartmentNumber)
PRINT '';
PRINT 'Vérification unicité (BuildingId + ApartmentNumber) :';
SELECT 
    BuildingId,
    ApartmentNumber,
    COUNT(*) AS [Nb Doublons]
FROM Apartments
GROUP BY BuildingId, ApartmentNumber
HAVING COUNT(*) > 1;

DECLARE @Duplicates INT;
SELECT @Duplicates = COUNT(*) 
FROM (
    SELECT BuildingId, ApartmentNumber
    FROM Apartments
    GROUP BY BuildingId, ApartmentNumber
    HAVING COUNT(*) > 1
) AS Dupes;

IF @Duplicates = 0
    PRINT '✓ Aucun doublon détecté - Contrainte respectée !';
ELSE
    PRINT '✗ ATTENTION : ' + CAST(@Duplicates AS VARCHAR(10)) + ' doublons détectés !';

PRINT '';
PRINT '=============================================';
PRINT 'Script terminé avec succès !';
PRINT '=============================================';
GO
