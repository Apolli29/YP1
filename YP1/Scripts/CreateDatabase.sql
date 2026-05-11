IF DB_ID(N'ReadWriteDontCheatDb') IS NULL
BEGIN
    CREATE DATABASE [ReadWriteDontCheatDb];
END
GO

USE [ReadWriteDontCheatDb];
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FullName NVARCHAR(120) NOT NULL,
        Login NVARCHAR(60) NOT NULL UNIQUE,
        Email NVARCHAR(120) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(128) NOT NULL,
        RoleName NVARCHAR(30) NOT NULL CONSTRAINT DF_Users_RoleName DEFAULT N'reader',
        IsFrozen BIT NOT NULL CONSTRAINT DF_Users_IsFrozen DEFAULT 0,
        FreezeReason NVARCHAR(500) NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT GETDATE()
    );
END
GO

IF OBJECT_ID(N'dbo.Genres', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Genres
    (
        GenreId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(80) NOT NULL UNIQUE
    );
END
GO

IF OBJECT_ID(N'dbo.Books', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Books
    (
        BookId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title NVARCHAR(150) NOT NULL,
        Description NVARCHAR(MAX) NOT NULL,
        BookText NVARCHAR(MAX) NOT NULL,
        CoverColor NVARCHAR(20) NOT NULL CONSTRAINT DF_Books_CoverColor DEFAULT N'#5E6C84',
        AuthorId INT NOT NULL,
        IsFrozen BIT NOT NULL CONSTRAINT DF_Books_IsFrozen DEFAULT 0,
        FreezeReason NVARCHAR(500) NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Books_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_Books_Users FOREIGN KEY (AuthorId) REFERENCES dbo.Users(UserId)
    );
END
GO

IF OBJECT_ID(N'dbo.BookGenres', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BookGenres
    (
        BookId INT NOT NULL,
        GenreId INT NOT NULL,
        CONSTRAINT PK_BookGenres PRIMARY KEY (BookId, GenreId),
        CONSTRAINT FK_BookGenres_Books FOREIGN KEY (BookId) REFERENCES dbo.Books(BookId) ON DELETE CASCADE,
        CONSTRAINT FK_BookGenres_Genres FOREIGN KEY (GenreId) REFERENCES dbo.Genres(GenreId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.Reviews', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reviews
    (
        ReviewId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        BookId INT NOT NULL,
        UserId INT NOT NULL,
        Rating INT NOT NULL,
        ReviewText NVARCHAR(1000) NOT NULL,
        IsFrozen BIT NOT NULL CONSTRAINT DF_Reviews_IsFrozen DEFAULT 0,
        FreezeReason NVARCHAR(500) NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Reviews_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT CK_Reviews_Rating CHECK (Rating BETWEEN 1 AND 5),
        CONSTRAINT FK_Reviews_Books FOREIGN KEY (BookId) REFERENCES dbo.Books(BookId),
        CONSTRAINT FK_Reviews_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
    );
END
GO

IF OBJECT_ID(N'dbo.BookListItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BookListItems
    (
        ItemId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL,
        BookId INT NOT NULL,
        ListName NVARCHAR(30) NOT NULL,
        UpdatedAt DATETIME NOT NULL CONSTRAINT DF_BookListItems_UpdatedAt DEFAULT GETDATE(),
        CONSTRAINT UQ_BookListItems UNIQUE (UserId, BookId),
        CONSTRAINT FK_BookListItems_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_BookListItems_Books FOREIGN KEY (BookId) REFERENCES dbo.Books(BookId)
    );
END
GO

IF OBJECT_ID(N'dbo.Reports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reports
    (
        ReportId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ReporterUserId INT NOT NULL,
        TargetType NVARCHAR(30) NOT NULL,
        TargetId INT NOT NULL,
        Reason NVARCHAR(500) NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Reports_Status DEFAULT N'pending',
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Reports_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_Reports_Users FOREIGN KEY (ReporterUserId) REFERENCES dbo.Users(UserId)
    );
END
GO

IF OBJECT_ID(N'dbo.AuthorApplications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuthorApplications
    (
        ApplicationId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL,
        Message NVARCHAR(500) NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_AuthorApplications_Status DEFAULT N'pending',
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_AuthorApplications_CreatedAt DEFAULT GETDATE(),
        ReviewedAt DATETIME NULL,
        CONSTRAINT FK_AuthorApplications_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
    );
END
GO

IF OBJECT_ID(N'dbo.FreezeAppeals', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FreezeAppeals
    (
        AppealId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL,
        EntityType NVARCHAR(30) NOT NULL,
        EntityId INT NOT NULL,
        AppealText NVARCHAR(500) NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_FreezeAppeals_Status DEFAULT N'pending',
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_FreezeAppeals_CreatedAt DEFAULT GETDATE(),
        ReviewedAt DATETIME NULL,
        CONSTRAINT FK_FreezeAppeals_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Genres)
BEGIN
    INSERT INTO dbo.Genres (Name)
    VALUES
        (N'Фэнтези'),
        (N'Детектив'),
        (N'Роман'),
        (N'Научная фантастика'),
        (N'Приключения'),
        (N'Психология'),
        (N'Драма');
END
GO
