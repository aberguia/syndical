-- ===================================================================
-- Migration: Add Supplier Management Features and Update Expenses
-- Date: 2024
-- Description: 
--   1. Add ServiceCategory and audit fields to Suppliers table
--   2. Remove CategoryId from Expenses table
--   3. Add foreign keys and indexes
-- ===================================================================

USE GestionSyndicale;
GO

-- ===================================================================
-- STEP 1: Update Suppliers Table
-- ===================================================================

-- Add new columns to Suppliers if they don't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'ServiceCategory')
BEGIN
    ALTER TABLE Suppliers ADD ServiceCategory NVARCHAR(100) NOT NULL DEFAULT 'Autre';
    PRINT 'Added ServiceCategory column to Suppliers table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'Description')
BEGIN
    ALTER TABLE Suppliers ADD Description NVARCHAR(500) NULL;
    PRINT 'Added Description column to Suppliers table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    ALTER TABLE Suppliers ADD IsDeleted BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to Suppliers table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'CreatedByUserId')
BEGIN
    ALTER TABLE Suppliers ADD CreatedByUserId INT NULL;
    PRINT 'Added CreatedByUserId column to Suppliers table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'UpdatedByUserId')
BEGIN
    ALTER TABLE Suppliers ADD UpdatedByUserId INT NULL;
    PRINT 'Added UpdatedByUserId column to Suppliers table';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'UpdatedOn')
BEGIN
    ALTER TABLE Suppliers ADD UpdatedOn DATETIME2 NULL;
    PRINT 'Added UpdatedOn column to Suppliers table';
END

-- Rename CreatedAt to CreatedOn if it exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'CreatedAt')
BEGIN
    EXEC sp_rename 'Suppliers.CreatedAt', 'CreatedOn', 'COLUMN';
    PRINT 'Renamed CreatedAt to CreatedOn in Suppliers table';
END
ELSE IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'CreatedOn')
BEGIN
    ALTER TABLE Suppliers ADD CreatedOn DATETIME2 NOT NULL DEFAULT GETDATE();
    PRINT 'Added CreatedOn column to Suppliers table';
END

-- ===================================================================
-- STEP 2: Add Foreign Keys for Suppliers Audit Trail
-- ===================================================================

-- Add FK for CreatedByUserId
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Suppliers_CreatedByUser')
BEGIN
    ALTER TABLE Suppliers
    ADD CONSTRAINT FK_Suppliers_CreatedByUser
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id);
    PRINT 'Added FK_Suppliers_CreatedByUser foreign key';
END

-- Add FK for UpdatedByUserId
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Suppliers_UpdatedByUser')
BEGIN
    ALTER TABLE Suppliers
    ADD CONSTRAINT FK_Suppliers_UpdatedByUser
    FOREIGN KEY (UpdatedByUserId) REFERENCES Users(Id);
    PRINT 'Added FK_Suppliers_UpdatedByUser foreign key';
END

-- ===================================================================
-- STEP 3: Add Unique Index on Suppliers (Name, ServiceCategory)
-- ===================================================================

-- Create unique index only after both columns exist
IF COL_LENGTH('Suppliers', 'ServiceCategory') IS NOT NULL 
   AND COL_LENGTH('Suppliers', 'IsDeleted') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Suppliers_Name_ServiceCategory')
    BEGIN
        CREATE UNIQUE INDEX IX_Suppliers_Name_ServiceCategory
        ON Suppliers(Name, ServiceCategory)
        WHERE IsDeleted = 0;
        PRINT 'Added unique index IX_Suppliers_Name_ServiceCategory';
    END
END

-- ===================================================================
-- STEP 4: Update Expenses Table - Remove CategoryId
-- ===================================================================

-- Drop foreign key first if it exists
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Expenses_ExpenseCategories' OR name = 'FK_Expenses_Categories')
BEGIN
    DECLARE @fkName NVARCHAR(255);
    SELECT @fkName = name FROM sys.foreign_keys 
    WHERE (name = 'FK_Expenses_ExpenseCategories' OR name = 'FK_Expenses_Categories')
    AND parent_object_id = OBJECT_ID('Expenses');
    
    IF @fkName IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE Expenses DROP CONSTRAINT ' + @fkName);
        PRINT 'Dropped foreign key constraint for CategoryId from Expenses';
    END
END

-- Drop CategoryId column if it exists
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Expenses' AND COLUMN_NAME = 'CategoryId')
BEGIN
    ALTER TABLE Expenses DROP COLUMN CategoryId;
    PRINT 'Removed CategoryId column from Expenses table';
END

-- ===================================================================
-- STEP 5: Ensure SupplierId column exists in Expenses
-- ===================================================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Expenses' AND COLUMN_NAME = 'SupplierId')
BEGIN
    ALTER TABLE Expenses ADD SupplierId INT NULL;
    PRINT 'Added SupplierId column to Expenses table';
END

-- Add FK for SupplierId if not exists
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Expenses_Suppliers')
BEGIN
    ALTER TABLE Expenses
    ADD CONSTRAINT FK_Expenses_Suppliers
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id);
    PRINT 'Added FK_Expenses_Suppliers foreign key';
END

-- ===================================================================
-- STEP 6: Update existing Suppliers with default ServiceCategory
-- ===================================================================

-- Set ServiceCategory for existing suppliers based on common patterns
-- Only if ServiceCategory column exists and has data
IF COL_LENGTH('Suppliers', 'ServiceCategory') IS NOT NULL
BEGIN
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
    WHERE ServiceCategory = 'Autre' OR ServiceCategory IS NULL;

    PRINT 'Updated ServiceCategory for existing suppliers';
END

-- ===================================================================
-- STEP 7: Seed some test suppliers (optional)
-- ===================================================================

-- Only insert if Suppliers table has the new columns and has few entries
IF COL_LENGTH('Suppliers', 'ServiceCategory') IS NOT NULL 
   AND COL_LENGTH('Suppliers', 'IsDeleted') IS NOT NULL
BEGIN
    DECLARE @supplierCount INT;
    SELECT @supplierCount = COUNT(*) FROM Suppliers WHERE IsDeleted = 0;

    IF @supplierCount < 3
    BEGIN
        INSERT INTO Suppliers (Name, ServiceCategory, Phone, Email, IsActive, IsDeleted, CreatedOn)
        VALUES 
            ('Plomberie Martin', 'Plomberie', '0612345678', 'contact@plomberie-martin.fr', 1, 0, GETDATE()),
            ('Électricité Pro', 'Électricité', '0623456789', 'info@electro-pro.fr', 1, 0, GETDATE()),
            ('Jardins Verts', 'Jardinage', '0634567890', 'contact@jardins-verts.fr', 1, 0, GETDATE()),
            ('Nettoyage Excellence', 'Nettoyage', '0645678901', 'contact@nettoyage-ex.fr', 1, 0, GETDATE()),
            ('Ascenseurs Sécurité', 'Ascenseur', '0656789012', 'service@ascenseurs-securite.fr', 1, 0, GETDATE());
        
        PRINT 'Inserted test suppliers';
    END
END

-- ===================================================================
-- STEP 8: Verification
-- ===================================================================

PRINT '';
PRINT '========================================';
PRINT 'Migration Completed Successfully!';
PRINT '========================================';
PRINT '';
PRINT 'Suppliers table structure:';
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Suppliers'
ORDER BY ORDINAL_POSITION;

PRINT '';

-- Display counts only if columns exist
IF COL_LENGTH('Suppliers', 'IsDeleted') IS NOT NULL
BEGIN
    DECLARE @totalSuppliers INT;
    SELECT @totalSuppliers = COUNT(*) FROM Suppliers WHERE IsDeleted = 0;
    PRINT 'Total Suppliers: ' + CAST(@totalSuppliers AS VARCHAR);
END
ELSE
BEGIN
    DECLARE @totalSuppliersAll INT;
    SELECT @totalSuppliersAll = COUNT(*) FROM Suppliers;
    PRINT 'Total Suppliers: ' + CAST(@totalSuppliersAll AS VARCHAR);
END

DECLARE @totalExpenses INT;
SELECT @totalExpenses = COUNT(*) FROM Expenses;
PRINT 'Total Expenses: ' + CAST(@totalExpenses AS VARCHAR);
PRINT '';

GO
