using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public partial class BookEditionRepository
{
    private const string AvailableCopiesSql = @"
        (be.TotalCopies - 
        (
            SELECT COUNT(*) 
            FROM Loan l 
            WHERE l.BookEditionID = be.BookEditionID AND l.ReturnDate IS NULL)
        ) AS AvailableCopies";

    private const string BaseQuery = $@"
        SELECT be.*, {AvailableCopiesSql}, b.*, p.*, l.*, t.*, a.*, g.*
        FROM BookEdition be
        LEFT JOIN Book b ON be.BookID = b.BookID
        LEFT JOIN Publisher p ON be.PublisherID = p.PublisherID
        LEFT JOIN Language l ON be.LanguageID = l.LanguageID
        LEFT JOIN BookEditionTranslator bet ON be.BookEditionID = bet.BookEditionID
        LEFT JOIN Translator t ON bet.TranslatorID = t.TranslatorID
        LEFT JOIN BookAuthor ba ON b.BookID = ba.BookID
        LEFT JOIN Author a ON ba.AuthorID = a.AuthorID
        LEFT JOIN BookGenre bg ON b.BookID = bg.BookID
        LEFT JOIN Genre g ON bg.GenreID = g.GenreID
        ";

    public async Task<int> GetCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM BookEdition";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetCountByGenreAsync(int genreId)
    {
        const string sql = @"
            SELECT COUNT(DISTINCT be.BookEditionID)
            FROM BookEdition be
            JOIN BookGenre bg ON be.BookID = bg.BookID
            WHERE bg.GenreID = @GenreID";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, new { GenreID = genreId });
    }

    public async Task<int> GetCountByLanguageAsync(int languageId)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM BookEdition
            WHERE LanguageID = @LanguageID";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, new { LanguageID = languageId });
    }

    public async Task<int> GetCountByTitleAsync(string title)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM BookEdition be
            JOIN Book b ON be.BookID = b.BookID
            WHERE be.EditionTitle LIKE @Title OR b.OriginalTitle LIKE @Title";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, new { Title = $"%{title}%" });
    }

    public async Task<int> GetCountByIsbnAsync(string isbn)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM BookEdition
            WHERE ISBN LIKE @ISBN";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql, new { ISBN = $"%{isbn}%" });
    }

    public async Task<BookEdition?> GetByIdAsync(int bookEditionId)
    {
        const string sql = $@"{BaseQuery}
            WHERE be.BookEditionID = @BookEditionID";

        using var connection = await CreateConnectionAsync();
        return (await GetBookEditionsWithRelationsAsync(connection, sql, new { BookEditionID = bookEditionId })).FirstOrDefault();
    }

    public async Task<List<BookEdition>> GetAllAsync()
    {
        const string sql = $@"{BaseQuery}
            ORDER BY be.EditionTitle"; 

        using var connection = await CreateConnectionAsync();
        return await GetBookEditionsWithRelationsAsync(connection, sql, null);
    }


    public async Task<List<BookEdition>> GetPageAsync(int pageNumber, int pageSize)
    {
        const string sql = $@"{BaseQuery} 
            ORDER BY be.EditionTitle
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        return await GetPagedResultAsync(sql, pageNumber, pageSize);
    }

    public async Task<List<BookEdition>> GetPageByGenreAsync(int genreId, int pageNumber, int pageSize)
    {
        const string sql = $@"{BaseQuery}
            WHERE bg.GenreID = @GenreID
            ORDER BY be.EditionTitle
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        return await GetPagedResultAsync(sql, pageNumber, pageSize, new { GenreID = genreId });
    }

    public async Task<List<BookEdition>> GetPageByLanguageAsync(int languageId, int pageNumber, int pageSize)
    {
        const string sql = $@"{BaseQuery}
            WHERE be.LanguageID = @LanguageID
            ORDER BY be.EditionTitle
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        return await GetPagedResultAsync(sql, pageNumber, pageSize, new { LanguageID = languageId });
    }

    public async Task<List<BookEdition>> GetPageByTitleAsync(string title, int pageNumber, int pageSize)
    {
        const string sql = $@"{BaseQuery}
            WHERE be.EditionTitle LIKE @Title OR b.OriginalTitle LIKE @Title
            ORDER BY be.EditionTitle
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        return await GetPagedResultAsync(sql, pageNumber, pageSize, new { Title = $"%{title}%" });
    }

    public async Task<List<BookEdition>> GetPageByIsbnAsync(string isbn, int pageNumber, int pageSize)
    {
        const string sql = $@"{BaseQuery}
            WHERE be.ISBN LIKE @ISBN
            ORDER BY be.EditionTitle
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        return await GetPagedResultAsync(sql, pageNumber, pageSize, new { ISBN = $"%{isbn}%" });
    }

    private async Task<List<BookEdition>> GetPagedResultAsync(string sql, int pageNumber, int pageSize, object? param = null)
    {
        using var connection = await CreateConnectionAsync();
        var paginationParams = new
        {
            Offset = (pageNumber - 1) * pageSize,
            PageSize = pageSize
        };

        var combinedParams = new DynamicParameters(param);
        combinedParams.AddDynamicParams(paginationParams);

        return await GetBookEditionsWithRelationsAsync(connection, sql, combinedParams);
    }

    private async Task<List<BookEdition>> GetBookEditionsWithRelationsAsync(IDbConnection connection, string sql, object? param)
    {
        var bookEditionDictionary = new Dictionary<int, BookEdition>();
        var bookDictionary = new Dictionary<int, Book>();

        await connection.QueryAsync<BookEdition, Book, Publisher, Language, Translator, Author, Genre, BookEdition>(
            sql,
            (bookEdition, book, publisher, language, translator, author, genre) =>
            {
                if (!bookEditionDictionary.TryGetValue(bookEdition.BookEditionID, out var bookEditionEntry))
                {
                    bookEditionEntry = bookEdition;
                    bookEditionDictionary.Add(bookEdition.BookEditionID, bookEditionEntry);
                    bookEditionEntry.Translators = new List<Translator>();
                }

                if (book != null)
                {
                    if (!bookDictionary.TryGetValue(book.BookID, out var bookEntry))
                    {
                        bookEntry = book;
                        bookDictionary.Add(book.BookID, bookEntry);
                        bookEntry.Authors = new List<Author>();
                        bookEntry.Genres = new List<Genre>();
                    }

                    bookEditionEntry.Book = bookEntry;

                    if (author != null && !bookEntry.Authors.Any(a => a.AuthorID == author.AuthorID))
                    {
                        bookEntry.Authors.Add(author);
                    }

                    if (genre != null && !bookEntry.Genres.Any(g => g.GenreID == genre.GenreID))
                    {
                        bookEntry.Genres.Add(genre);
                    }
                }

                if (translator != null && !bookEditionEntry.Translators.Any(t => t.TranslatorID == translator.TranslatorID))
                {
                    bookEditionEntry.Translators.Add(translator);
                }

                bookEditionEntry.Publisher = publisher;
                bookEditionEntry.Language = language;

                return bookEditionEntry;
            },
            param,
            splitOn: "BookID,PublisherID,LanguageID,TranslatorID,AuthorID,GenreID");

        return bookEditionDictionary.Values.ToList();
    }
}
