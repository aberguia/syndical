-- ==========================================
-- Migration: Annonces, Sondages, Dashboard
-- Date: 2025-12-25
-- ==========================================

-- Table: Announcements
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Announcements' AND xtype='U')
BEGIN
    CREATE TABLE Announcements (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Title NVARCHAR(200) NOT NULL,
        Body NVARCHAR(MAX) NOT NULL,
        Status INT NOT NULL DEFAULT 0, -- 0=Draft, 1=Published, 2=Archived
        CreatedByUserId INT NOT NULL,
        CreatedOn DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedByUserId INT NULL,
        UpdatedOn DATETIME2 NULL,
        CONSTRAINT FK_Announcements_CreatedByUser FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
        CONSTRAINT FK_Announcements_UpdatedByUser FOREIGN KEY (UpdatedByUserId) REFERENCES Users(Id)
    );

    CREATE INDEX IX_Announcements_Status ON Announcements(Status);
    CREATE INDEX IX_Announcements_CreatedOn ON Announcements(CreatedOn DESC);
    PRINT 'Table Announcements created successfully';
END
ELSE
BEGIN
    PRINT 'Table Announcements already exists';
END
GO

-- Table: Polls
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Polls' AND xtype='U')
BEGIN
    CREATE TABLE Polls (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Question NVARCHAR(300) NOT NULL,
        Status INT NOT NULL DEFAULT 0, -- 0=Draft, 1=Published, 2=Closed, 3=Archived
        CreatedByUserId INT NOT NULL,
        CreatedOn DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ClosedOn DATETIME2 NULL,
        CONSTRAINT FK_Polls_CreatedByUser FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
    );

    CREATE INDEX IX_Polls_Status ON Polls(Status);
    CREATE INDEX IX_Polls_CreatedOn ON Polls(CreatedOn DESC);
    PRINT 'Table Polls created successfully';
END
ELSE
BEGIN
    PRINT 'Table Polls already exists';
END
GO

-- Table: PollOptions
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PollOptions' AND xtype='U')
BEGIN
    CREATE TABLE PollOptions (
        Id INT PRIMARY KEY IDENTITY(1,1),
        PollId INT NOT NULL,
        Label NVARCHAR(200) NOT NULL,
        SortOrder INT NOT NULL DEFAULT 0,
        CONSTRAINT FK_PollOptions_Poll FOREIGN KEY (PollId) REFERENCES Polls(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_PollOptions_PollId ON PollOptions(PollId);
    CREATE INDEX IX_PollOptions_SortOrder ON PollOptions(PollId, SortOrder);
    PRINT 'Table PollOptions created successfully';
END
ELSE
BEGIN
    PRINT 'Table PollOptions already exists';
END
GO

-- Table: PollVotes
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PollVotes' AND xtype='U')
BEGIN
    CREATE TABLE PollVotes (
        Id INT PRIMARY KEY IDENTITY(1,1),
        PollId INT NOT NULL,
        PollOptionId INT NOT NULL,
        AdherentId INT NOT NULL,
        VotedOn DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_PollVotes_Poll FOREIGN KEY (PollId) REFERENCES Polls(Id) ON DELETE CASCADE,
        CONSTRAINT FK_PollVotes_PollOption FOREIGN KEY (PollOptionId) REFERENCES PollOptions(Id),
        CONSTRAINT FK_PollVotes_Adherent FOREIGN KEY (AdherentId) REFERENCES Users(Id)
    );

    -- Unique constraint: one vote per adherent per poll
    CREATE UNIQUE INDEX UQ_PollVotes_PollAdherent ON PollVotes(PollId, AdherentId);
    CREATE INDEX IX_PollVotes_PollId ON PollVotes(PollId);
    CREATE INDEX IX_PollVotes_AdherentId ON PollVotes(AdherentId);
    PRINT 'Table PollVotes created successfully';
END
ELSE
BEGIN
    PRINT 'Table PollVotes already exists';
END
GO

PRINT 'Migration completed successfully!';
