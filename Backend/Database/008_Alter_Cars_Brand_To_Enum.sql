-- Script de migration : Modification du champ Brand en INT (enum CarBrand)
-- Date: 2024-12-24

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Étape 1: Ajouter une nouvelle colonne temporaire
ALTER TABLE Cars ADD BrandNew INT NULL;
GO

-- Étape 2: Migrer les données existantes (si elles existent)
-- On met Dacia par défaut pour les voitures existantes
UPDATE Cars SET BrandNew = 0 WHERE Brand IS NULL OR Brand = '';
UPDATE Cars SET BrandNew = 0 WHERE Brand = 'Dacia';
UPDATE Cars SET BrandNew = 1 WHERE Brand = 'Renault';
UPDATE Cars SET BrandNew = 2 WHERE Brand = 'Peugeot';
UPDATE Cars SET BrandNew = 3 WHERE Brand = 'Citroën' OR Brand = 'Citroen';
UPDATE Cars SET BrandNew = 4 WHERE Brand = 'Hyundai';
UPDATE Cars SET BrandNew = 5 WHERE Brand = 'Kia';
UPDATE Cars SET BrandNew = 6 WHERE Brand = 'Toyota';
UPDATE Cars SET BrandNew = 7 WHERE Brand = 'Volkswagen';
UPDATE Cars SET BrandNew = 8 WHERE Brand = 'Mercedes';
UPDATE Cars SET BrandNew = 9 WHERE Brand = 'BMW';
UPDATE Cars SET BrandNew = 10 WHERE Brand = 'Audi';
UPDATE Cars SET BrandNew = 11 WHERE Brand = 'Ford';
UPDATE Cars SET BrandNew = 12 WHERE Brand = 'Fiat';
UPDATE Cars SET BrandNew = 13 WHERE Brand = 'Nissan';
UPDATE Cars SET BrandNew = 14 WHERE Brand = 'Suzuki';
UPDATE Cars SET BrandNew = 15 WHERE Brand = 'Opel';
UPDATE Cars SET BrandNew = 16 WHERE Brand = 'Seat';
UPDATE Cars SET BrandNew = 17 WHERE Brand = 'Skoda';
UPDATE Cars SET BrandNew = 18 WHERE Brand = 'Mazda';
UPDATE Cars SET BrandNew = 19 WHERE Brand = 'Mitsubishi';
-- Tout ce qui n'est pas reconnu = Autre
UPDATE Cars SET BrandNew = 99 WHERE BrandNew IS NULL;
GO

-- Étape 3: Supprimer l'ancienne colonne Brand
ALTER TABLE Cars DROP COLUMN Brand;
GO

-- Étape 4: Renommer la nouvelle colonne
EXEC sp_rename 'Cars.BrandNew', 'Brand', 'COLUMN';
GO

-- Étape 5: Rendre la colonne NOT NULL avec valeur par défaut
ALTER TABLE Cars ALTER COLUMN Brand INT NOT NULL;
GO

-- Étape 6: Ajouter une contrainte par défaut (Dacia = 0)
ALTER TABLE Cars ADD CONSTRAINT DF_Cars_Brand DEFAULT 0 FOR Brand;
GO

PRINT 'Migration terminée : Brand est maintenant un INT (enum CarBrand)';
