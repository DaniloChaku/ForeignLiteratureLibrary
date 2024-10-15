using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class BookRepository : BaseRepository, IBookRepository
{
    public BookRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task AddAsync(Book book)
    {
        using var connection = await CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertBookSql = @"
                INSERT INTO Book (OriginalTitle, OriginalLanguageCode, PublicationYear)
                OUTPUT INSERTED.BookID
                VALUES (@OriginalTitle, @OriginalLanguageCode, @PublicationYear)";

            book.BookID = await connection.QuerySingleAsync<int>(insertBookSql, book, transaction);

            await InsertBookAuthorsAsync(book, connection, transaction);
            await InsertBookGenresAsync(book, connection, transaction);

            transaction.Commit();
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_OriginalTitle"))
        {
            throw new CheckConstraintViolationException(
                "Cannot add the book because the title cannot be empty", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_PublicationYear"))
        {
            throw new CheckConstraintViolationException(
                $"Cannot add the book because the publication year '{book.PublicationYear}' is invalid", ex);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            if (ex.Message.Contains("FK_Book_Language", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ForeignKeyViolationException(
                    $"Cannot add the book because the language '{book.OriginalLanguageCode}' does not exist", ex);
            }
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                $"Cannot add the book because it already exists", ex);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
    public async Task UpdateAsync(Book book)
    {
        using var connection = await CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string updateBookSql = @"
                UPDATE Book 
                SET OriginalTitle = @OriginalTitle,
                    OriginalLanguageCode = @OriginalLanguageCode,
                    PublicationYear = @PublicationYear
                WHERE BookId = @BookId";

            await connection.ExecuteAsync(updateBookSql, book, transaction);

            await UpdateBookAuthorsAsync(book, connection, transaction);
            await UpdateBookGenresAsync(book, connection, transaction);

            transaction.Commit();
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_OriginalTitle"))
        {
            throw new CheckConstraintViolationException(
                "Cannot update the book because the title cannot be empty", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_PublicationYear"))
        {
            throw new CheckConstraintViolationException(
                $"Cannot update the book because the publication year '{book.PublicationYear}' is invalid", ex);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            if (ex.Message.Contains("FK_Book_Language", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ForeignKeyViolationException(
                    $"Cannot update the book because the language '{book.OriginalLanguageCode}' does not exist", ex);
            }
            throw;
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                $"Cannot update the book because it already exists", ex);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private async Task InsertBookAuthorsAsync(Book book, IDbConnection connection, IDbTransaction transaction)
    {
        if (book.Authors.Count == 0) return;

        const string insertAuthorsSql = @"
            INSERT INTO BookAuthor (BookID, AuthorID)
            VALUES (@BookId, @AuthorId)";

        var authorParams = book.Authors.Select(author => new { BookId = book.BookID, AuthorId = author.AuthorID });
        await connection.ExecuteAsync(insertAuthorsSql, authorParams, transaction);
    }

    private async Task InsertBookGenresAsync(Book book, IDbConnection connection, IDbTransaction transaction)
    {
        if (book.Genres.Count == 0) return;

        const string insertGenresSql = @"
            INSERT INTO BookGenre (BookID, GenreID)
            VALUES (@BookId, @GenreId)";

        var genreParams = book.Genres.Select(genre => new { BookId = book.BookID, GenreId = genre.GenreID });
        await connection.ExecuteAsync(insertGenresSql, genreParams, transaction);
    }

    private async Task UpdateBookAuthorsAsync(Book book, IDbConnection connection, IDbTransaction transaction)
    {
        await connection.ExecuteAsync("DELETE FROM BookAuthor WHERE BookID = @BookId", new { book.BookID }, transaction);
        await InsertBookAuthorsAsync(book, connection, transaction);
    }

    private async Task UpdateBookGenresAsync(Book book, IDbConnection connection, IDbTransaction transaction)
    {
        await connection.ExecuteAsync("DELETE FROM BookGenre WHERE BookID = @BookId", new { book.BookID }, transaction);
        await InsertBookGenresAsync(book, connection, transaction);
    }

    public async Task DeleteAsync(int bookId)
    {
        try
        {
            const string sql = "DELETE FROM Book WHERE BookId = @BookId";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, new { BookId = bookId });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new ForeignKeyViolationException(
                $"Cannot delete book '{bookId}' because it is referenced by other entities.", ex);
        }
    }

    public async Task<int> GetCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Book";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<Book?> GetByIdAsync(int bookId)
    {
        const string sql = @"
            SELECT b.*, l.*, a.*, g.*
            FROM Book b
            LEFT JOIN Language l ON b.OriginalLanguageCode = l.LanguageCode
            LEFT JOIN BookAuthor ba ON b.BookId = ba.BookId
            LEFT JOIN Author a ON ba.AuthorId = a.AuthorId
            LEFT JOIN BookGenre bg ON b.BookId = bg.BookId
            LEFT JOIN Genre g ON bg.GenreId = g.GenreId
            WHERE b.BookId = @BookId";

        var books = await QueryBooksAsync(sql, new { BookId = bookId });
        return books.FirstOrDefault();
    }

    public async Task<List<Book>> GetPageAsync(int pageNumber, int pageSize)
    {
        const string sql = @"
            SELECT b.*, l.*, a.*, g.*
            FROM Book b
            LEFT JOIN Language l ON b.OriginalLanguageCode = l.LanguageCode
            LEFT JOIN BookAuthor ba ON b.BookId = ba.BookId
            LEFT JOIN Author a ON ba.AuthorId = a.AuthorId
            LEFT JOIN BookGenre bg ON b.BookId = bg.BookId
            LEFT JOIN Genre g ON bg.GenreId = g.GenreId
            ORDER BY b.OriginalTitle
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var books = await QueryBooksAsync(sql, new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize });
        return books.ToList();
    }

    public async Task<List<Book>> GetAllAsync()
    {
        const string sql = @"
            SELECT b.*, l.*, a.*, g.*
            FROM Book b
            LEFT JOIN Language l ON b.OriginalLanguageCode = l.LanguageCode
            LEFT JOIN BookAuthor ba ON b.BookId = ba.BookId
            LEFT JOIN Author a ON ba.AuthorId = a.AuthorId
            LEFT JOIN BookGenre bg ON b.BookId = bg.BookId
            LEFT JOIN Genre g ON bg.GenreId = g.GenreId
            ORDER BY b.OriginalTitle";

        var books = await QueryBooksAsync(sql);
        return books.ToList();
    }

    private async Task<IEnumerable<Book>> QueryBooksAsync(string sql, object? parameters = null)
    {
        using var connection = await CreateConnectionAsync();
        var bookDictionary = new Dictionary<int, Book>();

        await connection.QueryAsync<Book, Language, Author, Genre, Book>(
            sql,
            (book, language, author, genre) =>
            {
                if (!bookDictionary.TryGetValue(book.BookID, out var bookEntry))
                {
                    bookEntry = book;
                    bookEntry.OriginalLanguage = language;
                    bookEntry.Authors = new List<Author>();
                    bookEntry.Genres = new List<Genre>();
                    bookDictionary.Add(bookEntry.BookID, bookEntry);
                }

                if (author != null && !bookEntry.Authors.Any(a => a.AuthorID == author.AuthorID))
                    bookEntry.Authors.Add(author);
                if (genre != null && !bookEntry.Genres.Any(g => g.GenreID == genre.GenreID))
                    bookEntry.Genres.Add(genre);

                return bookEntry;
            },
            parameters,
            splitOn: "LanguageCode,AuthorId,GenreId"
        );

        return bookDictionary.Values;
    }

    public async Task<List<TopBook>> GetTop10BooksAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
                SELECT TOP 10
                    b.BookId,
                    b.OriginalTitle,
                    COUNT(bel.BookEditionLoanID) as LoanCount
                FROM Book b
                JOIN BookEdition be ON b.BookId = be.BookId
                JOIN BookEditionLoan bel ON be.BookEditionID = bel.BookEditionID
                WHERE bel.LoanDate BETWEEN @StartDate AND @EndDate
                GROUP BY b.BookId, b.OriginalTitle
                ORDER BY LoanCount DESC";

        using var connection = await CreateConnectionAsync();
        return (await connection.QueryAsync<TopBook>(sql, new { StartDate = startDate, EndDate = endDate })).ToList();
    }
}
