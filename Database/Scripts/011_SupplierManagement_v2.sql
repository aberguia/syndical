-- ===================================================================
-- Migration: Add Supplier Management Features and Update Expenses
-- Date: 2024
-- ===================================================================

USE GestionSyndicale;
SET QUOTED_IDENTIFIER ON;
GO

PRINT 'Starting Supplier Management Migration...';
PRINT '';

-- ===================================================================
-- STEP 1: Add new columns to Suppliers table
-- ===================================================================

-- ServiceCategory
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'ServiceCategory')
BEGIN
    ALTER TABLE Suppliers ADD ServiceCategory NVARCHAR(100) NOT NULL DEFAULT 'Autre';
    PRINT '✓ Added ServiceCategory column';
END
ELSE
    PRINT '- ServiceCategory column already exists';

-- Description
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'Description')
BEGIN
    ALTER TABLE Suppliers ADD Description NVARCHAR(500) NULL;
    PRINT '✓ Added Description column';
END
ELSE
    PRINT '- Description column already exists';

-- IsDeleted
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE Suppliers ADD IsDeleted BIT NOT NULL DEFAULT 0;
    PRINT '✓ Added IsDeleted column';
END
ELSE
    PRINT '- IsDeleted column already exists';

-- CreatedByUserId
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'CreatedByUserId')
BEGIN
    ALTER TABLE Suppliers ADD CreatedByUserId INT NULL;
    PRINT '✓ Added CreatedByUserId column';
END
ELSE
    PRINT '- CreatedByUserId column already exists';

-- UpdatedByUserId
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'UpdatedByUserId')
BEGIN
    ALTER TABLE Suppliers ADD UpdatedByUserId INT NULL;
    PRINT '✓ Added UpdatedByUserId column';
END
ELSE
    PRINT '- UpdatedByUserId column already exists';

-- UpdatedOn
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'UpdatedOn')
BEGIN
    ALTER TABLE Suppliers ADD UpdatedOn DATETIME2 NULL;
    PRINT '✓ Added UpdatedOn column';
END
ELSE
    PRINT '- UpdatedOn column already exists';

-- Rename CreatedAt to CreatedOn
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'CreatedAt')
AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'CreatedOn')
BEGIN
    EXEC sp_rename 'Suppliers.CreatedAt', 'CreatedOn', 'COLUMN';
    PRINT '✓ Renamed CreatedAt to CreatedOn';
END
ELSE IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'CreatedOn')
BEGIN
    ALTER TABLE Suppliers ADD CreatedOn DATETIME2 NOT NULL DEFAULT GETDATE();
    PRINT '✓ Added CreatedOn column';
END
ELSE
    PRINT '- CreatedOn column already exists';

PRINT '';

-- ===================================================================
-- STEP 2: Add Foreign Keys
-- ===================================================================

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Suppliers_CreatedByUser')
BEGIN
    ALTER TABLE Suppliers
    ADD CONSTRAINT FK_Suppliers_CreatedByUser
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id);
    PRINT '✓ Added FK_Suppliers_CreatedByUser';
END
ELSE
    PRINT '- FK_Suppliers_CreatedByUser already exists';

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Suppliers_UpdatedByUser')
BEGIN
    ALTER TABLE Suppliers
    ADD CONSTRAINT FK_Suppliers_UpdatedByUser
    FOREIGN KEY (UpdatedByUserId) REFERENCES Users(Id);
    PRINT '✓ Added FK_Suppliers_UpdatedByUser';
END
ELSE
    PRINT '- FK_Suppliers_UpdatedByUser already exists';

PRINT '';

GO

-- ===================================================================
-- STEP 3: Update existing suppliers
-- ===================================================================

UPDATE Suppliers
SET ServiceCategory = CASE
    WHEN Name LIKE '%plomb%' THEN 'Plomberie'
    WHEN Name LIKE '%électr%' THEN 'Électricité'
    WHEN Name LIKE '%peinture%' THEN 'Peinture'
    WHEN Name LIKE '%jardin%' THEN 'Jardinage'
    WHEN Name LIKE '%nettoy%' THEN 'Nettoyage'
    WHEN Name LIKE '%ascens%' THEN 'Ascenseur'
    WHEN Name LIKE '%menuise%' THEN 'Menuiserie'
    WHEN Name LIKE '%sécur%' OR Name LIKE '%secur%' THEN 'Sécurité'
    WHEN Name LIKE '%climat%' THEN 'Climatisation'
    WHEN Name LIKE '%assur%' THEN 'Assurance'
    WHEN Name LIKE '%avocat%' OR Name LIKE '%juridi%' THEN 'Juridique'
    ELSE 'Autre'
END
WHERE ServiceCategory = 'Autre';

PRINT '✓ Updated ServiceCategory for existing suppliers';
PRINT '';

GO

-- ===================================================================
-- STEP 4: Add Unique Index
-- ===================================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Suppliers_Name_ServiceCategory' AND object_id = OBJECT_ID('Suppliers'))
BEGIN
    CREATE UNIQUE INDEX IX_Suppliers_Name_ServiceCategory
    ON Suppliers(Name, ServiceCategory)
    WHERE IsDeleted = 0;
    PRINT '✓ Added unique index IX_Suppliers_Name_ServiceCategory';
END
ELSE
    PRINT '- Index IX_Suppliers_Name_ServiceCategory already exists';

PRINT '';

-- ===================================================================
-- STEP 5: Update Expenses table
-- ===================================================================

-- Drop foreign key for CategoryId if exists
DECLARE @fkName NVARCHAR(255);
SELECT @fkName = name 
FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('Expenses')
AND referenced_object_id = OBJECT_ID('ExpenseCategories');

IF @fkName IS NOT NULL
BEGIN
    DECLARE @dropFKSql NVARCHAR(MAX) = 'ALTER TABLE Expenses DROP CONSTRAINT ' + QUOTENAME(@fkName);
    EXEC sp_executesql @dropFKSql;
    PRINT '✓ Dropped foreign key ' + @fkName + ' from Expenses';
END
ELSE
    PRINT '- No FK to ExpenseCategories found';

-- Drop CategoryId column
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Expenses') AND name = 'CategoryId')
BEGIN
    -- First, drop any indexes on CategoryId
    DECLARE @indexName NVARCHAR(255);
    SELECT @indexName = i.name
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    WHERE i.object_id = OBJECT_ID('Expenses')
    AND c.name = 'CategoryId'
    AND i.is_primary_key = 0;

    IF @indexName IS NOT NULL
    BEGIN
        DECLARE @dropIndexSql NVARCHAR(MAX) = 'DROP INDEX ' + QUOTENAME(@indexName) + ' ON Expenses';
        EXEC sp_executesql @dropIndexSql;
        PRINT '✓ Dropped index ' + @indexName + ' on CategoryId';
    END

    ALTER TABLE Expenses DROP COLUMN CategoryId;
    PRINT '✓ Removed CategoryId column from Expenses';
END
ELSE
    PRINT '- CategoryId column does not exist in Expenses';

-- Ensure SupplierId exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Expenses') AND name = 'SupplierId')
BEGIN
    ALTER TABLE Expenses ADD SupplierId INT NULL;
    PRINT '✓ Added SupplierId column to Expenses';
END
ELSE
    PRINT '- SupplierId column already exists';

-- Add FK for SupplierId
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Expenses_Suppliers')
BEGIN
    ALTER TABLE Expenses
    ADD CONSTRAINT FK_Expenses_Suppliers
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id);
    PRINT '✓ Added FK_Expenses_Suppliers';
END
ELSE
    PRINT '- FK_Expenses_Suppliers already exists';

PRINT '';

-- ===================================================================
-- STEP 6: Insert test suppliers
-- ===================================================================

DECLARE @supplierCount INT;
SELECT @supplierCount = COUNT(*) FROM Suppliers WHERE IsDeleted = 0;

IF @supplierCount < 3
BEGIN
    IF NOT EXISTS (SELECT * FROM Suppliers WHERE Name = 'Plomberie Martin')
        INSERT INTO Suppliers (Name, ServiceCategory, Phone, Email, IsActive, IsDeleted, CreatedOn)
        VALUES ('Plomberie Martin', 'Plomberie', '0612345678', 'contact@plomberie-martin.fr', 1, 0, GETDATE());
    
    IF NOT EXISTS (SELECT * FROM Suppliers WHERE Name = 'Électricité Pro')
        INSERT INTO Suppliers (Name, ServiceCategory, Phone, Email, IsActive, IsDeleted, CreatedOn)
        VALUES ('Électricité Pro', 'Électricité', '0623456789', 'info@electro-pro.fr', 1, 0, GETDATE());
    
    IF NOT EXISTS (SELECT * FROM Suppliers WHERE Name = 'Jardins Verts')
        INSERT INTO Suppliers (Name, ServiceCategory, Phone, Email, IsActive, IsDeleted, CreatedOn)
        VALUES ('Jardins Verts', 'Jardinage', '0634567890', 'contact@jardins-verts.fr', 1, 0, GETDATE());
    
    IF NOT EXISTS (SELECT * FROM Suppliers WHERE Name = 'Nettoyage Excellence')
        INSERT INTO Suppliers (Name, ServiceCategory, Phone, Email, IsActive, IsDeleted, CreatedOn)
        VALUES ('Nettoyage Excellence', 'Nettoyage', '0645678901', 'contact@nettoyage-ex.fr', 1, 0, GETDATE());
    
    IF NOT EXISTS (SELECT * FROM Suppliers WHERE Name = 'Ascenseurs Sécurité')
        INSERT INTO Suppliers (Name, ServiceCategory, Phone, Email, IsActive, IsDeleted, CreatedOn)
        VALUES ('Ascenseurs Sécurité', 'Ascenseur', '0656789012', 'service@ascenseurs-securite.fr', 1, 0, GETDATE());
    
    PRINT '✓ Inserted test suppliers';
END
ELSE
    PRINT '- Suppliers already exist, skipping seed data';

PRINT '';

-- ===================================================================
-- Verification
-- ===================================================================

DECLARE @finalSuppliers INT;
DECLARE @finalExpenses INT;

SELECT @finalSuppliers = COUNT(*) FROM Suppliers WHERE IsDeleted = 0;
SELECT @finalExpenses = COUNT(*) FROM Expenses;

PRINT '========================================';
PRINT 'Migration Completed Successfully!';
PRINT '========================================';
PRINT '';
PRINT 'Total Active Suppliers: ' + CAST(@finalSuppliers AS VARCHAR);
PRINT 'Total Expenses: ' + CAST(@finalExpenses AS VARCHAR);
PRINT '';
PRINT 'Suppliers table columns:';

SELECT 
    COLUMN_NAME as [Column], 
    DATA_TYPE as [Type],
    IS_NULLABLE as [Nullable]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Suppliers'
ORDER BY ORDINAL_POSITION;

GO
