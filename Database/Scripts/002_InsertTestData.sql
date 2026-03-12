-- =============================================
-- Script d'insertion de données de test
-- Application de Gestion Syndicale
-- =============================================

USE GestionSyndicale;
GO

PRINT 'Début insertion données de test...';

-- =============================================
-- 1. CRÉER LA RÉSIDENCE
-- =============================================

DECLARE @ResidenceId INT;

IF NOT EXISTS (SELECT 1 FROM Residences WHERE Name = 'Résidence Les Jardins')
BEGIN
    INSERT INTO Residences (Name, Address, City, PostalCode, Phone, Email, IsActive, CreatedAt)
    VALUES ('Résidence Les Jardins', '123 Avenue de la République', 'Paris', '75011', 
            '0145678910', 'contact@residence-jardins.fr', 1, GETUTCDATE());
    
    SET @ResidenceId = SCOPE_IDENTITY();
    PRINT 'Résidence créée avec ID: ' + CAST(@ResidenceId AS VARCHAR);
END
ELSE
BEGIN
    SET @ResidenceId = (SELECT Id FROM Residences WHERE Name = 'Résidence Les Jardins');
    PRINT 'Résidence existante avec ID: ' + CAST(@ResidenceId AS VARCHAR);
END

-- =============================================
-- 2. CRÉER LES IMMEUBLES
-- =============================================

DECLARE @BuildingAId INT, @BuildingBId INT, @BuildingCId INT;

-- Immeuble A
IF NOT EXISTS (SELECT 1 FROM Buildings WHERE ResidenceId = @ResidenceId AND BuildingNumber = 'A')
BEGIN
    INSERT INTO Buildings (ResidenceId, BuildingNumber, Name, FloorCount, IsActive, CreatedAt)
    VALUES (@ResidenceId, 'A', 'Bâtiment A', 5, 1, GETUTCDATE());
    SET @BuildingAId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SET @BuildingAId = (SELECT Id FROM Buildings WHERE ResidenceId = @ResidenceId AND BuildingNumber = 'A');
END

-- Immeuble B
IF NOT EXISTS (SELECT 1 FROM Buildings WHERE ResidenceId = @ResidenceId AND BuildingNumber = 'B')
BEGIN
    INSERT INTO Buildings (ResidenceId, BuildingNumber, Name, FloorCount, IsActive, CreatedAt)
    VALUES (@ResidenceId, 'B', 'Bâtiment B', 5, 1, GETUTCDATE());
    SET @BuildingBId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SET @BuildingBId = (SELECT Id FROM Buildings WHERE ResidenceId = @ResidenceId AND BuildingNumber = 'B');
END

-- Immeuble C
IF NOT EXISTS (SELECT 1 FROM Buildings WHERE ResidenceId = @ResidenceId AND BuildingNumber = 'C')
BEGIN
    INSERT INTO Buildings (ResidenceId, BuildingNumber, Name, FloorCount, IsActive, CreatedAt)
    VALUES (@ResidenceId, 'C', 'Bâtiment C', 4, 1, GETUTCDATE());
    SET @BuildingCId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SET @BuildingCId = (SELECT Id FROM Buildings WHERE ResidenceId = @ResidenceId AND BuildingNumber = 'C');
END

PRINT 'Immeubles créés: A=' + CAST(@BuildingAId AS VARCHAR) + ', B=' + CAST(@BuildingBId AS VARCHAR) + ', C=' + CAST(@BuildingCId AS VARCHAR);

-- =============================================
-- 3. CRÉER LES APPARTEMENTS
-- =============================================

-- Fonction helper pour créer appartements
DECLARE @AppartementsCrees INT = 0;

-- Immeuble A - 5 étages, 2 apparts/étage = 10 apparts
DECLARE @Etage INT = 1;
WHILE @Etage <= 5
BEGIN
    -- Appartement 01
    IF NOT EXISTS (SELECT 1 FROM Apartments WHERE BuildingId = @BuildingAId AND ApartmentNumber = CAST(@Etage AS VARCHAR) + '01')
    BEGIN
        INSERT INTO Apartments (BuildingId, ApartmentNumber, Floor, Surface, SharesCount, IsActive, CreatedAt)
        VALUES (@BuildingAId, CAST(@Etage AS VARCHAR) + '01', @Etage, 65.5 + (@Etage * 0.5), 100 + (@Etage * 2), 1, GETUTCDATE());
        SET @AppartementsCrees = @AppartementsCrees + 1;
    END
    
    -- Appartement 02
    IF NOT EXISTS (SELECT 1 FROM Apartments WHERE BuildingId = @BuildingAId AND ApartmentNumber = CAST(@Etage AS VARCHAR) + '02')
    BEGIN
        INSERT INTO Apartments (BuildingId, ApartmentNumber, Floor, Surface, SharesCount, IsActive, CreatedAt)
        VALUES (@BuildingAId, CAST(@Etage AS VARCHAR) + '02', @Etage, 55.0 + (@Etage * 0.5), 85 + (@Etage * 2), 1, GETUTCDATE());
        SET @AppartementsCrees = @AppartementsCrees + 1;
    END
    
    SET @Etage = @Etage + 1;
END

-- Immeuble B - 5 étages, 2 apparts/étage = 10 apparts
SET @Etage = 1;
WHILE @Etage <= 5
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Apartments WHERE BuildingId = @BuildingBId AND ApartmentNumber = CAST(@Etage AS VARCHAR) + '01')
    BEGIN
        INSERT INTO Apartments (BuildingId, ApartmentNumber, Floor, Surface, SharesCount, IsActive, CreatedAt)
        VALUES (@BuildingBId, CAST(@Etage AS VARCHAR) + '01', @Etage, 60.0, 90, 1, GETUTCDATE());
        SET @AppartementsCrees = @AppartementsCrees + 1;
    END
    
    IF NOT EXISTS (SELECT 1 FROM Apartments WHERE BuildingId = @BuildingBId AND ApartmentNumber = CAST(@Etage AS VARCHAR) + '02')
    BEGIN
        INSERT INTO Apartments (BuildingId, ApartmentNumber, Floor, Surface, SharesCount, IsActive, CreatedAt)
        VALUES (@BuildingBId, CAST(@Etage AS VARCHAR) + '02', @Etage, 58.0, 88, 1, GETUTCDATE());
        SET @AppartementsCrees = @AppartementsCrees + 1;
    END
    
    SET @Etage = @Etage + 1;
END

-- Immeuble C - 4 étages, 3 apparts/étage = 12 apparts
SET @Etage = 1;
WHILE @Etage <= 4
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Apartments WHERE BuildingId = @BuildingCId AND ApartmentNumber = CAST(@Etage AS VARCHAR) + '01')
    BEGIN
        INSERT INTO Apartments (BuildingId, ApartmentNumber, Floor, Surface, SharesCount, IsActive, CreatedAt)
        VALUES (@BuildingCId, CAST(@Etage AS VARCHAR) + '01', @Etage, 70.0, 105, 1, GETUTCDATE());
        SET @AppartementsCrees = @AppartementsCrees + 1;
    END
    
    IF NOT EXISTS (SELECT 1 FROM Apartments WHERE BuildingId = @BuildingCId AND ApartmentNumber = CAST(@Etage AS VARCHAR) + '02')
    BEGIN
        INSERT INTO Apartments (BuildingId, ApartmentNumber, Floor, Surface, SharesCount, IsActive, CreatedAt)
        VALUES (@BuildingCId, CAST(@Etage AS VARCHAR) + '02', @Etage, 62.0, 93, 1, GETUTCDATE());
        SET @AppartementsCrees = @AppartementsCrees + 1;
    END
    
    IF NOT EXISTS (SELECT 1 FROM Apartments WHERE BuildingId = @BuildingCId AND ApartmentNumber = CAST(@Etage AS VARCHAR) + '03')
    BEGIN
        INSERT INTO Apartments (BuildingId, ApartmentNumber, Floor, Surface, SharesCount, IsActive, CreatedAt)
        VALUES (@BuildingCId, CAST(@Etage AS VARCHAR) + '03', @Etage, 75.0, 112, 1, GETUTCDATE());
        SET @AppartementsCrees = @AppartementsCrees + 1;
    END
    
    SET @Etage = @Etage + 1;
END

PRINT 'Appartements créés: ' + CAST(@AppartementsCrees AS VARCHAR);

-- =============================================
-- 4. CRÉER UN SUPER ADMIN
-- =============================================

DECLARE @SuperAdminId INT;
DECLARE @SuperAdminRoleId INT = (SELECT Id FROM Roles WHERE Name = 'SuperAdmin');

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@residence-jardins.fr')
BEGIN
    -- Note: PasswordHash correspond à "Admin123!" hashé en SHA256
    -- En production, utiliser BCrypt ou un algorithme plus sécurisé
    INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone, ApartmentId, 
                       IsEmailConfirmed, IsActive, CreatedAt)
    VALUES ('admin@residence-jardins.fr', 
            '8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918', -- Admin123!
            'Jean', 'Dupuis', '0601020304', NULL, 1, 1, GETUTCDATE());
    
    SET @SuperAdminId = SCOPE_IDENTITY();
    
    -- Assigner le rôle SuperAdmin
    INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
    VALUES (@SuperAdminId, @SuperAdminRoleId, GETUTCDATE());
    
    PRINT 'Super Admin créé: admin@residence-jardins.fr (Password: Admin123!)';
END
ELSE
BEGIN
    PRINT 'Super Admin existe déjà';
END

-- =============================================
-- 5. CRÉER UN ADMIN
-- =============================================

DECLARE @AdminId INT;
DECLARE @AdminRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Admin');

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin2@residence-jardins.fr')
BEGIN
    INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone, ApartmentId, 
                       IsEmailConfirmed, IsActive, CreatedAt)
    VALUES ('admin2@residence-jardins.fr', 
            '8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918',
            'Marie', 'Martin', '0607080910', NULL, 1, 1, GETUTCDATE());
    
    SET @AdminId = SCOPE_IDENTITY();
    
    INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
    VALUES (@AdminId, @AdminRoleId, GETUTCDATE());
    
    PRINT 'Admin créé: admin2@residence-jardins.fr (Password: Admin123!)';
END

-- =============================================
-- 6. CRÉER QUELQUES ADHÉRENTS
-- =============================================

DECLARE @AdherentRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Adherent');
DECLARE @Apt1Id INT = (SELECT TOP 1 Id FROM Apartments WHERE BuildingId = @BuildingAId AND ApartmentNumber = '101');
DECLARE @Apt2Id INT = (SELECT TOP 1 Id FROM Apartments WHERE BuildingId = @BuildingAId AND ApartmentNumber = '201');
DECLARE @Apt3Id INT = (SELECT TOP 1 Id FROM Apartments WHERE BuildingId = @BuildingBId AND ApartmentNumber = '101');

-- Adhérent 1
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'pierre.bernard@email.com')
BEGIN
    DECLARE @User1Id INT;
    INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone, ApartmentId, 
                       IsEmailConfirmed, IsActive, CreatedAt)
    VALUES ('pierre.bernard@email.com', 
            '8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918',
            'Pierre', 'Bernard', '0611223344', @Apt1Id, 1, 1, GETUTCDATE());
    
    SET @User1Id = SCOPE_IDENTITY();
    INSERT INTO UserRoles (UserId, RoleId) VALUES (@User1Id, @AdherentRoleId);
    PRINT 'Adhérent créé: pierre.bernard@email.com (Appt A-101)';
END

-- Adhérent 2
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'sophie.durand@email.com')
BEGIN
    DECLARE @User2Id INT;
    INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone, ApartmentId, 
                       IsEmailConfirmed, IsActive, CreatedAt)
    VALUES ('sophie.durand@email.com', 
            '8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918',
            'Sophie', 'Durand', '0622334455', @Apt2Id, 1, 1, GETUTCDATE());
    
    SET @User2Id = SCOPE_IDENTITY();
    INSERT INTO UserRoles (UserId, RoleId) VALUES (@User2Id, @AdherentRoleId);
    PRINT 'Adhérent créé: sophie.durand@email.com (Appt A-201)';
END

-- Adhérent 3
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'luc.petit@email.com')
BEGIN
    DECLARE @User3Id INT;
    INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone, ApartmentId, 
                       IsEmailConfirmed, IsActive, CreatedAt)
    VALUES ('luc.petit@email.com', 
            '8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918',
            'Luc', 'Petit', '0633445566', @Apt3Id, 1, 1, GETUTCDATE());
    
    SET @User3Id = SCOPE_IDENTITY();
    INSERT INTO UserRoles (UserId, RoleId) VALUES (@User3Id, @AdherentRoleId);
    PRINT 'Adhérent créé: luc.petit@email.com (Appt B-101)';
END

-- =============================================
-- 7. CRÉER DES CHARGES
-- =============================================

DECLARE @Charge1Id INT, @Charge2Id INT;

IF NOT EXISTS (SELECT 1 FROM Charges WHERE Name = 'Charges mensuelles décembre 2024')
BEGIN
    INSERT INTO Charges (Name, Description, Amount, ChargeType, EffectiveDate, IsActive, CreatedAt, CreatedByUserId)
    VALUES ('Charges mensuelles décembre 2024', 
            'Entretien, eau, électricité parties communes', 
            3000.00, 'Monthly', '2024-12-01', 1, GETUTCDATE(), @SuperAdminId);
    SET @Charge1Id = SCOPE_IDENTITY();
    PRINT 'Charge créée: Charges décembre 2024';
END

IF NOT EXISTS (SELECT 1 FROM Charges WHERE Name = 'Charges mensuelles janvier 2025')
BEGIN
    INSERT INTO Charges (Name, Description, Amount, ChargeType, EffectiveDate, IsActive, CreatedAt, CreatedByUserId)
    VALUES ('Charges mensuelles janvier 2025', 
            'Entretien, eau, électricité parties communes', 
            3000.00, 'Monthly', '2025-01-01', 1, GETUTCDATE(), @SuperAdminId);
    SET @Charge2Id = SCOPE_IDENTITY();
    PRINT 'Charge créée: Charges janvier 2025';
END

-- =============================================
-- 8. CRÉER APPELS DE FONDS
-- =============================================

-- Calculer les appels de fonds pour tous les appartements
-- Exemple simplifié: répartition selon tantièmes

DECLARE @TotalShares INT = (SELECT SUM(SharesCount) FROM Apartments WHERE IsActive = 1);

-- Pour la charge de décembre
IF @Charge1Id IS NOT NULL
BEGIN
    INSERT INTO CallsForFunds (ChargeId, ApartmentId, AmountDue, AmountPaid, AmountRemaining, DueDate, Status, CreatedAt)
    SELECT 
        @Charge1Id,
        Id,
        ROUND(3000.00 * (SharesCount * 1.0 / @TotalShares), 2),
        0,
        ROUND(3000.00 * (SharesCount * 1.0 / @TotalShares), 2),
        '2024-12-31',
        'Pending',
        GETUTCDATE()
    FROM Apartments
    WHERE IsActive = 1;
    
    PRINT 'Appels de fonds créés pour décembre 2024';
END

-- Pour la charge de janvier
IF @Charge2Id IS NOT NULL
BEGIN
    INSERT INTO CallsForFunds (ChargeId, ApartmentId, AmountDue, AmountPaid, AmountRemaining, DueDate, Status, CreatedAt)
    SELECT 
        @Charge2Id,
        Id,
        ROUND(3000.00 * (SharesCount * 1.0 / @TotalShares), 2),
        0,
        ROUND(3000.00 * (SharesCount * 1.0 / @TotalShares), 2),
        '2025-01-31',
        'Pending',
        GETUTCDATE()
    FROM Apartments
    WHERE IsActive = 1;
    
    PRINT 'Appels de fonds créés pour janvier 2025';
END

-- =============================================
-- 9. STATISTIQUES FINALES
-- =============================================

-- Récupérer les statistiques dans des variables
DECLARE @CountResidences INT, @CountBuildings INT, @CountApartments INT;
DECLARE @CountUsers INT, @CountSuperAdmins INT, @CountAdmins INT, @CountAdherents INT;
DECLARE @CountCharges INT, @CountCallsForFunds INT;

SELECT @CountResidences = COUNT(*) FROM Residences;
SELECT @CountBuildings = COUNT(*) FROM Buildings;
SELECT @CountApartments = COUNT(*) FROM Apartments;
SELECT @CountUsers = COUNT(*) FROM Users;
SELECT @CountSuperAdmins = COUNT(*) FROM UserRoles WHERE RoleId = @SuperAdminRoleId;
SELECT @CountAdmins = COUNT(*) FROM UserRoles WHERE RoleId = @AdminRoleId;
SELECT @CountAdherents = COUNT(*) FROM UserRoles WHERE RoleId = @AdherentRoleId;
SELECT @CountCharges = COUNT(*) FROM Charges;
SELECT @CountCallsForFunds = COUNT(*) FROM CallsForFunds;

PRINT '';
PRINT '========== STATISTIQUES ==========';
PRINT 'Résidences: ' + CAST(@CountResidences AS VARCHAR);
PRINT 'Immeubles: ' + CAST(@CountBuildings AS VARCHAR);
PRINT 'Appartements: ' + CAST(@CountApartments AS VARCHAR);
PRINT 'Utilisateurs: ' + CAST(@CountUsers AS VARCHAR);
PRINT '  - SuperAdmin: ' + CAST(@CountSuperAdmins AS VARCHAR);
PRINT '  - Admin: ' + CAST(@CountAdmins AS VARCHAR);
PRINT '  - Adhérents: ' + CAST(@CountAdherents AS VARCHAR);
PRINT 'Charges: ' + CAST(@CountCharges AS VARCHAR);
PRINT 'Appels de fonds: ' + CAST(@CountCallsForFunds AS VARCHAR);
PRINT '';
PRINT '========== COMPTES DE TEST ==========';
PRINT 'Super Admin: admin@residence-jardins.fr / Admin123!';
PRINT 'Admin: admin2@residence-jardins.fr / Admin123!';
PRINT 'Adhérent 1: pierre.bernard@email.com / Admin123! (A-101)';
PRINT 'Adhérent 2: sophie.durand@email.com / Admin123! (A-201)';
PRINT 'Adhérent 3: luc.petit@email.com / Admin123! (B-101)';
PRINT '';
PRINT 'Script terminé avec succès!';

GO
