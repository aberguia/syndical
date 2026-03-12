-- Script de migration : Création de la table MemberNotes
-- Date: 2024-12-24

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Création de la table MemberNotes
CREATE TABLE MemberNotes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MemberId INT NOT NULL,
    NoteText NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedByUserId INT NULL,
    UpdatedAt DATETIME2 NULL,
    UpdatedByUserId INT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_MemberNotes_Member FOREIGN KEY (MemberId) REFERENCES Users(Id),
    CONSTRAINT FK_MemberNotes_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    CONSTRAINT FK_MemberNotes_UpdatedBy FOREIGN KEY (UpdatedByUserId) REFERENCES Users(Id)
);
GO

-- Index pour améliorer les performances
CREATE INDEX IX_MemberNotes_MemberId ON MemberNotes(MemberId);
CREATE INDEX IX_MemberNotes_CreatedAt ON MemberNotes(CreatedAt DESC);
CREATE INDEX IX_MemberNotes_IsDeleted ON MemberNotes(IsDeleted);
GO

PRINT 'Table MemberNotes créée avec succès';
