-- Script de migration : Création de la table Cars pour la gestion du parking
-- Date: 2025-12-24

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Création de la table Cars
CREATE TABLE Cars (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Brand NVARCHAR(100) NULL,
    PlatePart1 INT NOT NULL,
    PlatePart2 NVARCHAR(10) NOT NULL,
    PlatePart3 INT NOT NULL,
    CarType INT NOT NULL DEFAULT 0, -- 0=Primary, 1=Tenant, 2=Visitor
    MemberId INT NOT NULL,
    Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_Cars_Members FOREIGN KEY (MemberId) REFERENCES Users(Id)
);

-- Index unique sur la plaque (PlatePart1, PlatePart2, PlatePart3)
CREATE UNIQUE INDEX IX_Cars_Plate 
ON Cars(PlatePart1, PlatePart2, PlatePart3)
WHERE IsDeleted = 0;

-- Index pour améliorer les performances de recherche
CREATE INDEX IX_Cars_MemberId ON Cars(MemberId);
CREATE INDEX IX_Cars_CreatedAt ON Cars(CreatedAt DESC);
CREATE INDEX IX_Cars_IsDeleted ON Cars(IsDeleted);

PRINT 'Table Cars créée avec succès';
GO
