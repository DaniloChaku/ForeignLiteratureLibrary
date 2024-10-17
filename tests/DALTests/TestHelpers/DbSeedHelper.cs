using Dapper;
using Microsoft.Data.SqlClient;

namespace DALTests.TestHelpers;

public static class DbSeedHelper
{
    public static void SeedDb(string connectionString)
    {
        DropTables(connectionString);

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        var query = @"
        CREATE TABLE" + "\"Language\"" + @"(
            LanguageCode VARCHAR(3) PRIMARY KEY,
            Name NVARCHAR(50) NOT NULL UNIQUE,
            CONSTRAINT CHK_Language_LanguageCode CHECK (LEN(LTRIM(RTRIM(LanguageCode))) > 1),
            CONSTRAINT CHK_Language_Name CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
        );

        CREATE TABLE Country(
            CountryCode VARCHAR(3) PRIMARY KEY,
            Name NVARCHAR(100) NOT NULL UNIQUE,
            CONSTRAINT CHK_Country_CountryCode CHECK (LEN(LTRIM(RTRIM(CountryCode))) > 1),
            CONSTRAINT CHK_Country_Name CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
        );

        CREATE TABLE Publisher(
            PublisherID INT PRIMARY KEY IDENTITY(1, 1),
            Name NVARCHAR(255) NOT NULL,
            CONSTRAINT CHK_Publisher_Name CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
        );

        CREATE TABLE Book(
            BookID INT PRIMARY KEY IDENTITY(1, 1),
            OriginalTitle NVARCHAR(255) NOT NULL,
            OriginalLanguageCode VARCHAR(3) NOT NULL,
            PublicationYear INT NOT NULL,
            CONSTRAINT FK_Book_Language FOREIGN KEY(OriginalLanguageCode) REFERENCES" + "\"Language\"" + @"(LanguageCode),
            CONSTRAINT CHK_OriginalTitle CHECK (LEN(LTRIM(RTRIM(OriginalTitle))) > 0),
            CONSTRAINT CHK_PublicationYear CHECK(PublicationYear BETWEEN 0 AND YEAR(GETDATE()))
        );

        CREATE TABLE BookEdition(
            BookEditionID INT PRIMARY KEY IDENTITY(1, 1),
            ISBN VARCHAR(17) UNIQUE,
            BookID INT NOT NULL,
            Title NVARCHAR(255) NOT NULL,
            LanguageCode VARCHAR(3) NOT NULL,
            PageCount INT NOT NULL,
            ShelfLocation NVARCHAR(60) NOT NULL,
            TotalCopies INT NOT NULL,
            PublisherID INT,
            CONSTRAINT FK_BookEdition_Book FOREIGN KEY(BookID) REFERENCES Book(BookID),
            CONSTRAINT FK_BookEdition_Language FOREIGN KEY(LanguageCode) REFERENCES Language(LanguageCode),
            CONSTRAINT FK_BookEdition_Publisher FOREIGN KEY(PublisherID) REFERENCES Publisher(PublisherID),
            CONSTRAINT CHK_ISBN CHECK(LEN(LTRIM(RTRIM(ISBN))) > 0),
            CONSTRAINT CHK_Title CHECK (LEN(LTRIM(RTRIM(Title))) > 0),
            CONSTRAINT CHK_TotalCopies CHECK(TotalCopies >= 0),
            CONSTRAINT CHK_PageCount CHECK(PageCount > 0),
        );

        CREATE TABLE Author(
            AuthorID INT PRIMARY KEY IDENTITY(1, 1),
            FullName NVARCHAR(100) NOT NULL,
            CountryCode VARCHAR(3) NOT NULL,
            CONSTRAINT CHK_Author_FullName CHECK (LEN(LTRIM(RTRIM(FullName))) > 0),
            CONSTRAINT FK_Author_Country FOREIGN KEY(CountryCode) REFERENCES Country(CountryCode)
        );

        CREATE TABLE Translator(
            TranslatorID INT PRIMARY KEY IDENTITY(1, 1),
            FullName NVARCHAR(100) NOT NULL,
            CONSTRAINT CHK_Translator_FullName CHECK (LEN(LTRIM(RTRIM(FullName))) > 0)
        );

        CREATE TABLE Genre(
            GenreID INT PRIMARY KEY IDENTITY(1, 1),
            Name NVARCHAR(50) NOT NULL UNIQUE,
            CONSTRAINT CHK_Genre_Name CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
        );

        CREATE TABLE Reader(
            LibraryCardNumber VARCHAR(20) PRIMARY KEY,
            FullName NVARCHAR(100) NOT NULL,
            DateOfBirth DATE NOT NULL,
            Email VARCHAR(255),
            Phone VARCHAR(20),
            RegistrationDate DATE NOT NULL,
            CONSTRAINT CHK_FullName CHECK (LEN(LTRIM(RTRIM(FullName))) > 0),
            CONSTRAINT CHK_DateOfBirth CHECK(DateOfBirth >= '1900-01-01' AND DateOfBirth <= GETDATE()),
            CONSTRAINT CHK_RegistrationDate CHECK(RegistrationDate > '1900-01-01' AND RegistrationDate <= GETDATE()),
        );

        CREATE TABLE BookEditionLoan(
            BookEditionLoanID INT PRIMARY KEY IDENTITY(1, 1),
            BookEditionID INT NOT NULL,
            LibraryCardNumber VARCHAR(20) NOT NULL,
            LoanDate DATE NOT NULL,
            DueDate DATE NOT NULL,
            ReturnDate DATE,
            CONSTRAINT FK_BookLoan_BookEdition FOREIGN KEY(BookEditionID) REFERENCES BookEdition(BookEditionID),
            CONSTRAINT FK_BookLoan_Reader FOREIGN KEY(LibraryCardNumber) REFERENCES Reader(LibraryCardNumber),
            CONSTRAINT CHK_DueDate CHECK(DueDate >= LoanDate),
            CONSTRAINT CHK_ReturnDate CHECK(ReturnDate IS NULL OR ReturnDate >= LoanDate)
        );

        CREATE TABLE BookAuthor(
            BookID INT,
            AuthorID INT,
            PRIMARY KEY(BookID, AuthorID),
            CONSTRAINT FK_BookAuthor_Book FOREIGN KEY(BookID) REFERENCES Book(BookID) ON DELETE CASCADE,
            CONSTRAINT FK_BookAuthor_Author FOREIGN KEY(AuthorID) REFERENCES Author(AuthorID) ON DELETE CASCADE
        );

        CREATE TABLE BookEditionTranslator(
            BookEditionID INT,
            TranslatorID INT,
            PRIMARY KEY(BookEditionID, TranslatorID),
            CONSTRAINT FK_BookTranslator_BookEdition FOREIGN KEY(BookEditionID) REFERENCES BookEdition(BookEditionID) ON DELETE CASCADE,
            CONSTRAINT FK_BookTranslator_Translator FOREIGN KEY(TranslatorID) REFERENCES Translator(TranslatorID) ON DELETE CASCADE
        );

        CREATE TABLE BookGenre(
            BookID INT,
            GenreID INT,
            PRIMARY KEY(BookID, GenreID),
            CONSTRAINT FK_BookGenre_Book FOREIGN KEY(BookID) REFERENCES Book(BookID) ON DELETE CASCADE,
            CONSTRAINT FK_BookGenre_Genre FOREIGN KEY(GenreID) REFERENCES Genre(GenreID) ON DELETE CASCADE
        );

        -- Insert into Language
        INSERT INTO ""Language"" (LanguageCode, Name) VALUES
        ('EN', 'English'),
        ('FR', 'French'),
        ('DE', 'German'),
        ('UA', 'Ukrainian');

        -- Insert into Country
        INSERT INTO Country (CountryCode, Name) VALUES
        ('US', 'United States'),
        ('FR', 'France'),
        ('DE', 'Germany'),
        ('UA', 'Ukraine');

        -- Insert into Publisher
        INSERT INTO Publisher (Name) VALUES
        ('Penguin Books'),
        ('Hachette Livre'),
        ('HarperCollins'),
        ('Simon and Schuster');

        -- Insert into Book
        INSERT INTO Book (OriginalTitle, OriginalLanguageCode, PublicationYear) VALUES
        ('The Catcher in the Rye', 'EN', 1951),
        (N'Les Misérables', 'FR', 1862),
        ('Faust', 'DE', 1808),
        ('Nine Stories', 'EN', 1953);

        -- Insert into BookEdition
        INSERT INTO BookEdition (ISBN, BookID, Title, LanguageCode, PageCount, ShelfLocation, TotalCopies, PublisherID) VALUES
        ('978-0-14-023750-4', 1, 'The Catcher in the Rye', 'EN', 214, 'A1-01', 10, 1),
        ('978-0-19-953640-5', 2, 'Les Misérables', 'FR', 1463, 'B2-12', 8, 2),
        ('978-0-521-31009-0', 3, 'Faust', 'DE', 504, 'C3-04', 7, 3),
        ('978-0-521-31009-2', 3, 'Faust', 'DE', 504, 'C3-04', 5, 3);

        -- Insert into Author
        INSERT INTO Author (FullName, CountryCode) VALUES
        ('J.D. Salinger', 'US'),
        ('Victor Hugo', 'FR'),
        ('Johann Wolfgang von Goethe', 'DE'),
        ('Klaus Mann', 'DE');

        -- Insert into Translator
        INSERT INTO Translator (FullName) VALUES
        ('Charles Wilbour'),
        ('Philip Wayne'),
        ('Charles Baudelaire');

        -- Insert into Genre
        INSERT INTO Genre (Name) VALUES
        ('Fiction'),
        ('Philosophy'),
        ('Historical Novel'),
        ('Science Fiction');

        -- Insert into Reader
        INSERT INTO Reader (LibraryCardNumber, FullName, DateOfBirth, Email, Phone, RegistrationDate) VALUES
        ('1001', 'John Doe', '1990-05-15', 'john.doe@example.com', '+123456789', '2023-09-15'),
        ('1002', 'Jane Smith', '1985-08-23', 'jane.smith@example.com', '+987654321', '2023-10-01'),
        ('1003', 'Jane Doe', '1983-11-23', 'jane.doe@example.com', '+5681231', '2023-10-01');

        -- Insert into BookEditionLoan
        INSERT INTO BookEditionLoan (BookEditionID, LibraryCardNumber, LoanDate, DueDate, ReturnDate) VALUES
        (1, '1001', '2023-09-20', '2023-10-10', NULL),
        (2, '1002', '2023-10-05', '2023-10-25', NULL);

        -- Insert into BookAuthor
        INSERT INTO BookAuthor (BookID, AuthorID) VALUES
        (1, 1),
        (2, 2),
        (3, 3),
        (4, 1);

        -- Insert into BookEditionTranslator
        INSERT INTO BookEditionTranslator (BookEditionID, TranslatorID) VALUES
        (2, 1),
        (3, 2);

        -- Insert into BookGenre
        INSERT INTO BookGenre (BookID, GenreID) VALUES
        (1, 1),
        (2, 3),
        (3, 2),
        (4, 1);";

        var triggerBookEditionTotalCount = @"
        CREATE TRIGGER trg_EnsureAvailableCopies
        ON BookEdition
        AFTER UPDATE
        AS
        BEGIN
            SET NOCOUNT ON;

            DECLARE @BookEditionID INT, @NewTotalCopies INT, @OldTotalCopies INT, @OpenLoans INT;

            -- Handle multiple row operations
            IF (SELECT COUNT(*) FROM inserted) > 1
            BEGIN
                RAISERROR('This trigger does not support multi-row operations.', 16, 1);
                ROLLBACK TRANSACTION;
                RETURN;
            END

            SELECT @BookEditionID = i.BookEditionID, @NewTotalCopies = i.TotalCopies, @OldTotalCopies = d.TotalCopies
            FROM inserted i
            JOIN deleted d ON i.BookEditionID = d.BookEditionID;

            -- Only check if TotalCopies is being reduced
            IF @NewTotalCopies < @OldTotalCopies
            BEGIN
                SELECT @OpenLoans = COUNT(*)
                FROM BookEditionLoan
                WHERE BookEditionID = @BookEditionID AND ReturnDate IS NULL;

                IF (@NewTotalCopies - @OpenLoans < 0)
                BEGIN
                    DECLARE @ErrorMsg NVARCHAR(200) = 'Error: Total copies (' + CAST(@NewTotalCopies AS NVARCHAR) + 
                                                      ') cannot be less than open loans (' + CAST(@OpenLoans AS NVARCHAR) + 
                                                      ') for BookEditionID ' + CAST(@BookEditionID AS NVARCHAR) + '.';
                    RAISERROR(@ErrorMsg, 16, 1);
                    ROLLBACK TRANSACTION;
                END
            END
        END;
        ";

        var triggerBookEditionLoanPreventManualInsert = @"
        CREATE TRIGGER trg_PreventManualInsertOnLoan
        ON BookEditionLoan
        INSTEAD OF INSERT
        AS
        BEGIN
            SET NOCOUNT ON;

            DECLARE @BookEditionID INT, @LoanCount INT, @TotalCopies INT;
   
            -- Handle multiple row inserts
            IF (SELECT COUNT(*) FROM inserted) > 1
            BEGIN
                RAISERROR('This trigger does not support multi-row inserts.', 16, 1);
                RETURN;
            END

            SELECT @BookEditionID = BookEditionID FROM inserted;
    
            SELECT @LoanCount = COUNT(*)
            FROM BookEditionLoan
            WHERE BookEditionID = @BookEditionID AND ReturnDate IS NULL;

            SELECT @TotalCopies = TotalCopies 
            FROM BookEdition 
            WHERE BookEditionID = @BookEditionID;

            IF (@TotalCopies - @LoanCount > 0)
            BEGIN
                -- If copies are available, allow the insert
                INSERT INTO BookEditionLoan (BookEditionID, LibraryCardNumber, LoanDate, DueDate, ReturnDate)
                OUTPUT inserted.BookEditionLoanID
                SELECT BookEditionID, LibraryCardNumber, LoanDate, DueDate, ReturnDate 
                FROM inserted;
            END
            ELSE
            BEGIN
                -- If no available copies, raise an error
                RAISERROR('Error: No available copies for this book edition.', 16, 1);
            END
        END;
        ";

        connection.Execute(query);
        connection.Execute(triggerBookEditionTotalCount);
        connection.Execute(triggerBookEditionLoanPreventManualInsert);
    }

    public static void DropTables(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        var dropQuery = @"
        IF OBJECT_ID('dbo.BookEditionTranslator', 'U') IS NOT NULL DROP TABLE dbo.BookEditionTranslator;
        IF OBJECT_ID('dbo.BookGenre', 'U') IS NOT NULL DROP TABLE dbo.BookGenre;
        IF OBJECT_ID('dbo.BookAuthor', 'U') IS NOT NULL DROP TABLE dbo.BookAuthor;
        IF OBJECT_ID('dbo.BookEditionLoan', 'U') IS NOT NULL DROP TABLE dbo.BookEditionLoan;
        IF OBJECT_ID('dbo.BookEdition', 'U') IS NOT NULL DROP TABLE dbo.BookEdition;
        IF OBJECT_ID('dbo.Reader', 'U') IS NOT NULL DROP TABLE dbo.Reader;
        IF OBJECT_ID('dbo.Translator', 'U') IS NOT NULL DROP TABLE dbo.Translator;
        IF OBJECT_ID('dbo.Author', 'U') IS NOT NULL DROP TABLE dbo.Author;
        IF OBJECT_ID('dbo.Book', 'U') IS NOT NULL DROP TABLE dbo.Book;
        IF OBJECT_ID('dbo.Publisher', 'U') IS NOT NULL DROP TABLE dbo.Publisher;
        IF OBJECT_ID('dbo.Country', 'U') IS NOT NULL DROP TABLE dbo.Country;
        IF OBJECT_ID('dbo.Language', 'U') IS NOT NULL DROP TABLE dbo.Language;
        IF OBJECT_ID('dbo.Genre', 'U') IS NOT NULL DROP TABLE dbo.Genre;";

        connection.Execute(dropQuery);
    }
}
