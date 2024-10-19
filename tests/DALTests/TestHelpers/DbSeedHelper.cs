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
        CREATE TABLE ""Language"" (
            LanguageID INT PRIMARY KEY IDENTITY(1, 1),
            LanguageName NVARCHAR(50) NOT NULL UNIQUE,
            CONSTRAINT CHK_LanguageName CHECK (LEN(LTRIM(RTRIM(LanguageName))) > 0)
        );

        CREATE TABLE Country (
            CountryID INT PRIMARY KEY IDENTITY(1, 1),
            CountryName NVARCHAR(100) NOT NULL UNIQUE,
            CONSTRAINT CHK_CountryName CHECK (LEN(LTRIM(RTRIM(CountryName))) > 0)
        );

        CREATE TABLE Publisher (
            PublisherID INT PRIMARY KEY IDENTITY(1, 1),
            PublisherName NVARCHAR(100) NOT NULL,
            CountryID INT,
            CONSTRAINT CHK_PublisherName CHECK (LEN(LTRIM(RTRIM(PublisherName))) > 0),
            CONSTRAINT FK_Publisher_CountryID FOREIGN KEY(CountryID) REFERENCES Country(CountryID)
        );

        CREATE TABLE Book (
            BookID INT PRIMARY KEY IDENTITY(1, 1),
            OriginalTitle NVARCHAR(200) NOT NULL,
            OriginalLanguageID INT NOT NULL,
            FirstPublicationYear INT NOT NULL,
            BookDescription NVARCHAR(1500),
            CONSTRAINT FK_Book_OriginalLanguageID FOREIGN KEY(OriginalLanguageID) REFERENCES ""Language""(LanguageID),
            CONSTRAINT CHK_Book_OriginalTitle CHECK (LEN(LTRIM(RTRIM(OriginalTitle))) > 0),
            CONSTRAINT CHK_Book_FirstPublicationYear CHECK(FirstPublicationYear > 0)
        );

        CREATE TABLE Author (
            AuthorID INT PRIMARY KEY IDENTITY(1, 1),
            AuthorFullName NVARCHAR(100) NOT NULL,
            CountryID INT NOT NULL,
            BirthYear INT,
            DeathYear INT,
            CONSTRAINT CHK_Author_AuthorFullName CHECK (LEN(LTRIM(RTRIM(AuthorFullName))) > 0),
            CONSTRAINT FK_Author_CountryID FOREIGN KEY(CountryID) REFERENCES Country(CountryID),
            CONSTRAINT CHK_Author_BirthDeathYear CHECK (BirthYear IS NULL OR DeathYear IS NULL OR BirthYear <= DeathYear)
        );

        CREATE TABLE Genre (
            GenreID INT PRIMARY KEY IDENTITY(1, 1),
            GenreName NVARCHAR(50) NOT NULL UNIQUE,
            CONSTRAINT CHK_GenreName CHECK (LEN(LTRIM(RTRIM(GenreName))) > 0)
        );

        CREATE TABLE BookAuthor (
            BookID INT,
            AuthorID INT,
            PRIMARY KEY(BookID, AuthorID),
            CONSTRAINT FK_BookAuthor_BookID FOREIGN KEY(BookID) REFERENCES Book(BookID) ON DELETE CASCADE,
            CONSTRAINT FK_BookAuthor_AuthorID FOREIGN KEY(AuthorID) REFERENCES Author(AuthorID) ON DELETE CASCADE
        );

        CREATE TABLE BookGenre (
            BookID INT,
            GenreID INT,
            PRIMARY KEY(BookID, GenreID),
            CONSTRAINT FK_BookGenre_BookID FOREIGN KEY(BookID) REFERENCES Book(BookID) ON DELETE CASCADE,
            CONSTRAINT FK_BookGenre_GenreID FOREIGN KEY(GenreID) REFERENCES Genre(GenreID) ON DELETE CASCADE
        );

        CREATE TABLE BookEdition (
            BookEditionID INT PRIMARY KEY IDENTITY(1, 1),
            ISBN VARCHAR(17) UNIQUE,
            BookID INT NOT NULL,
            EditionTitle NVARCHAR(200) NOT NULL,
            LanguageID INT NOT NULL,
            PageCount INT NOT NULL,
            ShelfLocation NVARCHAR(30) NOT NULL,
            TotalCopies INT NOT NULL,
            PublisherID INT,
            EditionPublicationYear INT NOT NULL,
            CONSTRAINT FK_BookEdition_BookID FOREIGN KEY(BookID) REFERENCES Book(BookID),
            CONSTRAINT FK_BookEdition_LanguageID FOREIGN KEY(LanguageID) REFERENCES ""Language""(LanguageID),
            CONSTRAINT FK_BookEdition_PublisherID FOREIGN KEY(PublisherID) REFERENCES Publisher(PublisherID),
            CONSTRAINT CHK_BookEdition_ISBN CHECK(LEN(LTRIM(RTRIM(ISBN))) > 0),
            CONSTRAINT CHK_BookEdition_EditionTitle CHECK (LEN(LTRIM(RTRIM(EditionTitle))) > 0),
            CONSTRAINT CHK_BookEdition_TotalCopies CHECK(TotalCopies >= 0),
            CONSTRAINT CHK_BookEdition_PageCount CHECK(PageCount > 0),
            CONSTRAINT CHK_BookEdition_EditionPublicationYear CHECK(EditionPublicationYear > 0)
        );

        CREATE TABLE Translator (
            TranslatorID INT PRIMARY KEY IDENTITY(1, 1),
            TranslatorFullName NVARCHAR(100) NOT NULL,
            CountryID INT,
            CONSTRAINT CHK_Translator_TranslatorFullName CHECK (LEN(LTRIM(RTRIM(TranslatorFullName))) > 0),
            CONSTRAINT FK_Translator_CountryID FOREIGN KEY(CountryID) REFERENCES Country(CountryID)
        );

        CREATE TABLE BookEditionTranslator (
            BookEditionID INT,
            TranslatorID INT,
            PRIMARY KEY(BookEditionID, TranslatorID),
            CONSTRAINT FK_BookEditionTranslator_BookEditionID FOREIGN KEY(BookEditionID) REFERENCES BookEdition(BookEditionID) ON DELETE CASCADE,
            CONSTRAINT FK_BookEditionTranslator_TranslatorID FOREIGN KEY(TranslatorID) REFERENCES Translator(TranslatorID) ON DELETE CASCADE
        );

        CREATE TABLE Reader (
            ReaderID INT PRIMARY KEY IDENTITY(1, 1),
            LibraryCardNumber VARCHAR(20) NOT NULL UNIQUE,
            ReaderFullName NVARCHAR(100) NOT NULL,
            EmailAddress VARCHAR(100),
            PhoneNumber VARCHAR(20),
            CONSTRAINT CHK_Reader_ReaderFullName CHECK (LEN(LTRIM(RTRIM(ReaderFullName))) > 0)
        );

        CREATE TABLE Loan (
            LoanID INT PRIMARY KEY IDENTITY(1, 1),
            BookEditionID INT NOT NULL,
            ReaderID INT NOT NULL,
            LoanDate DATE NOT NULL,
            DueDate DATE NOT NULL,
            ReturnDate DATE,
            CONSTRAINT FK_Loan_BookEditionID FOREIGN KEY(BookEditionID) REFERENCES BookEdition(BookEditionID),
            CONSTRAINT FK_Loan_ReaderID FOREIGN KEY(ReaderID) REFERENCES Reader(ReaderID),
            CONSTRAINT CHK_Loan_DueDate CHECK(DueDate >= LoanDate),
            CONSTRAINT CHK_Loan_ReturnDate CHECK(ReturnDate IS NULL OR ReturnDate >= LoanDate)
        );


        -- Insert into Language
        INSERT INTO Language (LanguageName) VALUES
        ('English'),
        ('French'),
        ('German'),
        ('Ukrainian');

        -- Insert into Country
        INSERT INTO Country (CountryName) VALUES
        ('United States'),
        ('France'),
        ('Germany'),
        ('Ukraine');

        -- Insert into Publisher
        INSERT INTO Publisher (PublisherName, CountryID) VALUES
        ('Penguin Books', (SELECT CountryID FROM Country WHERE CountryName = 'United States')),
        ('Hachette Livre', (SELECT CountryID FROM Country WHERE CountryName = 'France')),
        ('HarperCollins', (SELECT CountryID FROM Country WHERE CountryName = 'United States')),
        ('Simon and Schuster', (SELECT CountryID FROM Country WHERE CountryName = 'United States'));

        -- Insert into Book
        INSERT INTO Book (OriginalTitle, OriginalLanguageID, FirstPublicationYear, BookDescription) VALUES
        ('The Catcher in the Rye', (SELECT LanguageID FROM Language WHERE LanguageName = 'English'), 1951, 'A novel by J.D. Salinger.'),
        (N'Les Misérables', (SELECT LanguageID FROM Language WHERE LanguageName = 'French'), 1862, 'A novel by Victor Hugo.'),
        ('Faust', (SELECT LanguageID FROM Language WHERE LanguageName = 'German'), 1808, 'A work by Johann Wolfgang von Goethe.'),
        ('Nine Stories', (SELECT LanguageID FROM Language WHERE LanguageName = 'English'), 1953, 'A collection of short stories by J.D. Salinger.');

        -- Insert into BookEdition
        INSERT INTO BookEdition (ISBN, BookID, EditionTitle, LanguageID, PageCount, ShelfLocation, TotalCopies, PublisherID, EditionPublicationYear) VALUES
        ('978-0-14-023750-4', 1, 'The Catcher in the Rye', (SELECT LanguageID FROM Language WHERE LanguageName = 'English'), 214, 'A1-01', 10, (SELECT PublisherID FROM Publisher WHERE PublisherName = 'Penguin Books'), 1951),
        ('978-0-19-953640-5', 2, 'Les Misérables', (SELECT LanguageID FROM Language WHERE LanguageName = 'French'), 1463, 'B2-12', 8, (SELECT PublisherID FROM Publisher WHERE PublisherName = 'Hachette Livre'), 1862),
        ('978-0-521-31009-0', 3, 'Faust', (SELECT LanguageID FROM Language WHERE LanguageName = 'German'), 504, 'C3-04', 7, (SELECT PublisherID FROM Publisher WHERE PublisherName = 'HarperCollins'), 1808),
        ('978-0-521-31009-2', 3, 'Faust', (SELECT LanguageID FROM Language WHERE LanguageName = 'German'), 504, 'C3-04', 5, (SELECT PublisherID FROM Publisher WHERE PublisherName = 'HarperCollins'), 1808);

        -- Insert into Author
        INSERT INTO Author (AuthorFullName, CountryID, BirthYear, DeathYear) VALUES
        ('J.D. Salinger', (SELECT CountryID FROM Country WHERE CountryName = 'United States'), 1919, 2010),
        ('Victor Hugo', (SELECT CountryID FROM Country WHERE CountryName = 'France'), 1802, 1885),
        ('Johann Wolfgang von Goethe', (SELECT CountryID FROM Country WHERE CountryName = 'Germany'), 1749, 1832),
        ('Klaus Mann', (SELECT CountryID FROM Country WHERE CountryName = 'Germany'), 1906, 1949);

        -- Insert into Translator
        INSERT INTO Translator (TranslatorFullName, CountryID) VALUES
        ('Charles Wilbour',1),
        ('Philip Wayne', 2),
        ('Charles Baudelaire', 3);

        -- Insert into Genre
        INSERT INTO Genre (GenreName) VALUES
        ('Fiction'),
        ('Philosophy'),
        ('Historical Novel'),
        ('Science Fiction');

        -- Insert into Reader
        INSERT INTO Reader (LibraryCardNumber, ReaderFullName, EmailAddress, PhoneNumber) VALUES
        ('1001', 'John Doe', 'john.doe@example.com', '+123456789'),
        ('1002', 'Jane Smith', 'jane.smith@example.com', '+987654321'),
        ('1003', 'Jane Doe', 'jane.doe@example.com', '+5681231');

        -- Insert into Loan
        INSERT INTO Loan (BookEditionID, ReaderID, LoanDate, DueDate, ReturnDate) VALUES
        (1, (SELECT ReaderID FROM Reader WHERE LibraryCardNumber = '1001'), '2023-09-20', '2023-10-10', NULL),
        (2, (SELECT ReaderID FROM Reader WHERE LibraryCardNumber = '1002'), '2023-10-05', '2023-10-25', NULL);

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
        (4, 1);
        ";

        var triggerEnforceLoanAvailability = @"
        CREATE TRIGGER trg_EnforceLoanAvailability
        ON Loan
        INSTEAD OF INSERT, UPDATE
        AS
        BEGIN
            SET NOCOUNT ON;
            DECLARE @BookEditionID INT, @LoanCount INT, @TotalCopies INT, @IsInsert BIT;

            SET @IsInsert = IIF(EXISTS(SELECT 1 FROM deleted), 0, 1);
   
            -- Handle multiple row operations
            IF (SELECT COUNT(*) FROM inserted) > 1
            BEGIN
                RAISERROR('This trigger does not support multi-row operations.', 16, 1);
                RETURN;
            END

            SELECT @BookEditionID = BookEditionID FROM inserted;
    
            -- For UPDATE, check availability if ReturnDate is being set to NULL
            IF @IsInsert = 0 AND (SELECT ReturnDate FROM inserted) IS NOT NULL
            BEGIN
                -- Perform the update without additional checks
                UPDATE l
                SET l.BookEditionID = i.BookEditionID,
                    l.ReaderID = i.ReaderID,
                    l.LoanDate = i.LoanDate,
                    l.DueDate = i.DueDate,
                    l.ReturnDate = i.ReturnDate
                FROM Loan l
                INNER JOIN inserted i ON l.LoanID = i.LoanID;

                RETURN;
            END

            -- Check availability
            SELECT @LoanCount = COUNT(*)
            FROM Loan
            WHERE BookEditionID = @BookEditionID AND ReturnDate IS NULL;

            SELECT @TotalCopies = TotalCopies 
            FROM BookEdition 
            WHERE BookEditionID = @BookEditionID;

            -- For UPDATE, exclude the current loan from the count if it's already counted
            IF @IsInsert = 0
            BEGIN
                SET @LoanCount = @LoanCount - 
                    CASE WHEN (SELECT ReturnDate FROM deleted) IS NULL THEN 1 ELSE 0 END;
            END

            IF (@TotalCopies - @LoanCount > 0)
            BEGIN
                -- If copies are available, allow the operation
                IF @IsInsert = 1
                BEGIN
                    INSERT INTO Loan (BookEditionID, ReaderID, LoanDate, DueDate, ReturnDate)
                    SELECT BookEditionID, ReaderID, LoanDate, DueDate, ReturnDate 
                    FROM inserted;

                    -- Return the new ID
                    SELECT SCOPE_IDENTITY() AS NewID;
                END
                ELSE
                BEGIN
                    UPDATE l
                    SET l.BookEditionID = i.BookEditionID,
                        l.ReaderID = i.ReaderID,
                        l.LoanDate = i.LoanDate,
                        l.DueDate = i.DueDate,
                        l.ReturnDate = i.ReturnDate
                    FROM Loan l
                    INNER JOIN inserted i ON l.LoanID = i.LoanID;
                END
            END
            ELSE
            BEGIN
                -- If no available copies, raise an error
                RAISERROR('Error: No available copies for this book edition.', 16, 1);
            END
        END;
        ";

        var triggerEnsureAvailableCopies = @"
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
                FROM Loan
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

        connection.Execute(query);
        connection.Execute(triggerEnforceLoanAvailability);
        connection.Execute(triggerEnsureAvailableCopies);
    }

    public static void DropTables(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        var dropQuery = @"
        IF OBJECT_ID('dbo.BookEditionTranslator', 'U') IS NOT NULL DROP TABLE dbo.BookEditionTranslator;
        IF OBJECT_ID('dbo.BookGenre', 'U') IS NOT NULL DROP TABLE dbo.BookGenre;
        IF OBJECT_ID('dbo.BookAuthor', 'U') IS NOT NULL DROP TABLE dbo.BookAuthor;
        IF OBJECT_ID('dbo.Loan', 'U') IS NOT NULL DROP TABLE dbo.Loan;
        IF OBJECT_ID('dbo.BookEdition', 'U') IS NOT NULL DROP TABLE dbo.BookEdition;
        IF OBJECT_ID('dbo.Reader', 'U') IS NOT NULL DROP TABLE dbo.Reader;
        IF OBJECT_ID('dbo.Translator', 'U') IS NOT NULL DROP TABLE dbo.Translator;
        IF OBJECT_ID('dbo.Author', 'U') IS NOT NULL DROP TABLE dbo.Author;
        IF OBJECT_ID('dbo.Book', 'U') IS NOT NULL DROP TABLE dbo.Book;
        IF OBJECT_ID('dbo.Publisher', 'U') IS NOT NULL DROP TABLE dbo.Publisher;
        IF OBJECT_ID('dbo.Country', 'U') IS NOT NULL DROP TABLE dbo.Country;
        IF OBJECT_ID('dbo.Language', 'U') IS NOT NULL DROP TABLE dbo.Language;
        IF OBJECT_ID('dbo.Genre', 'U') IS NOT NULL DROP TABLE dbo.Genre;
        ";

        connection.Execute(dropQuery);
    }
}
