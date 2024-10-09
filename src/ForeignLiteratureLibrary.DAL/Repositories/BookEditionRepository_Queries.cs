using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using System.Data;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public partial class BookEditionRepository
{
    public async Task<int> GetCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM BookEdition";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<BookEdition?> GetByIdAsync(int bookEditionId)
    {
        const string sql = @"
            SELECT be.*, b.*, p.*, l.*, t.*, a.*, g.*
            FROM BookEdition be
            LEFT JOIN Book b ON be.BookID = b.BookID
            LEFT JOIN Publisher p ON be.PublisherID = p.PublisherID
            LEFT JOIN Language l ON be.LanguageCode = l.LanguageCode
            LEFT JOIN BookEditionTranslator bet ON be.BookEditionID = bet.BookEditionID
            LEFT JOIN Translator t ON bet.TranslatorID = t.TranslatorID
            LEFT JOIN BookAuthor ba ON b.BookID = ba.BookID
            LEFT JOIN Author a ON ba.AuthorID = a.AuthorID
            LEFT JOIN BookGenre bg ON b.BookID = bg.BookID
            LEFT JOIN Genre g ON bg.GenreID = g.GenreID
            WHERE be.BookEditionID = @BookEditionID";

        return await GetBookEditionWithRelations(sql, new { BookEditionID = bookEditionId });
    }

    public async Task<BookEdition?> GetByIsbnAsync(string isbn)
    {
        const string sql = @"
            SELECT be.*, b.*, p.*, l.*, t.*, a.*, g.*
            FROM BookEdition be
            LEFT JOIN Book b ON be.BookID = b.BookID
            LEFT JOIN Publisher p ON be.PublisherID = p.PublisherID
            LEFT JOIN Language l ON be.LanguageCode = l.LanguageCode
            LEFT JOIN BookEditionTranslator bet ON be.BookEditionID = bet.BookEditionID
            LEFT JOIN Translator t ON bet.TranslatorID = t.TranslatorID
            LEFT JOIN BookAuthor ba ON b.BookID = ba.BookID
            LEFT JOIN Author a ON ba.AuthorID = a.AuthorID
            LEFT JOIN BookGenre bg ON b.BookID = bg.BookID
            LEFT JOIN Genre g ON bg.GenreID = g.GenreID
            WHERE be.ISBN = @ISBN";

        return await GetBookEditionWithRelations(sql, new { ISBN = isbn });
    }

    public async Task<List<BookEdition>> GetPageAsync(int pageNumber, int pageSize)
    {
        const string sql = @"
                SELECT be.*, b.*, p.*, t.*, a.*, g.*
                FROM BookEdition be
                LEFT JOIN Book b ON be.BookID = b.BookID
                LEFT JOIN Publisher p ON be.PublisherID = p.PublisherID
                LEFT JOIN BookEditionTranslator bet ON be.BookEditionID = bet.BookEditionID
                LEFT JOIN Translator t ON bet.TranslatorID = t.TranslatorID
                LEFT JOIN BookAuthor ba ON b.BookID = ba.BookID
                LEFT JOIN Author a ON ba.AuthorID = a.AuthorID
                LEFT JOIN BookGenre bg ON b.BookID = bg.BookID
                LEFT JOIN Genre g ON bg.GenreID = g.GenreID
                ORDER BY be.Title
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        return await GetPagedResults(sql, pageNumber, pageSize);
    }

    public async Task<List<BookEdition>> GetPageByGenreAsync(int genreId)
    {
        const string sql = @"
                SELECT DISTINCT be.*, b.*, p.*, t.*, a.*, g.*
                FROM BookEdition be
                JOIN Book b ON be.BookID = b.BookID
                JOIN BookGenre bg ON b.BookID = bg.BookID
                LEFT JOIN Publisher p ON be.PublisherID = p.PublisherID
                LEFT JOIN BookEditionTranslator bet ON be.BookEditionID = bet.BookEditionID
                LEFT JOIN Translator t ON bet.TranslatorID = t.TranslatorID
                LEFT JOIN BookAuthor ba ON b.BookID = ba.BookID
                LEFT JOIN Author a ON ba.AuthorID = a.AuthorID
                LEFT JOIN Genre g ON bg.GenreID = g.GenreID
                WHERE bg.GenreID = @GenreID
                ORDER BY be.Title";

        using var connection = await CreateConnectionAsync();
        return await GetBookEditionsWithRelations(connection, sql, new { GenreID = genreId });
    }

    public async Task<List<BookEdition>> GetPageByLanguageAsync(string languageCode)
    {
        const string sql = @"
                SELECT be.*, b.*, p.*, t.*, a.*, g.*
                FROM BookEdition be
                LEFT JOIN Book b ON be.BookID = b.BookID
                LEFT JOIN Publisher p ON be.PublisherID = p.PublisherID
                LEFT JOIN BookEditionTranslator bet ON be.BookEditionID = bet.BookEditionID
                LEFT JOIN Translator t ON bet.TranslatorID = t.TranslatorID
                LEFT JOIN BookAuthor ba ON b.BookID = ba.BookID
                LEFT JOIN Author a ON ba.AuthorID = a.AuthorID
                LEFT JOIN BookGenre bg ON b.BookID = bg.BookID
                LEFT JOIN Genre g ON bg.GenreID = g.GenreID
                WHERE be.LanguageCode = @LanguageCode
                ORDER BY be.Title";

        using var connection = await CreateConnectionAsync();
        return await GetBookEditionsWithRelations(connection, sql, new { LanguageCode = languageCode });
    }

    public async Task<List<BookEdition>> GetPageByTitleAsync(string title)
    {
        const string sql = @"
                SELECT be.*, b.*, p.*, t.*, a.*, g.*
                FROM BookEdition be
                LEFT JOIN Book b ON be.BookID = b.BookID
                LEFT JOIN Publisher p ON be.PublisherID = p.PublisherID
                LEFT JOIN BookEditionTranslator bet ON be.BookEditionID = bet.BookEditionID
                LEFT JOIN Translator t ON bet.TranslatorID = t.TranslatorID
                LEFT JOIN BookAuthor ba ON b.BookID = ba.BookID
                LEFT JOIN Author a ON ba.AuthorID = a.AuthorID
                LEFT JOIN BookGenre bg ON b.BookID = bg.BookID
                LEFT JOIN Genre g ON bg.GenreID = g.GenreID
                WHERE be.Title LIKE @Title OR b.OriginalTitle LIKE @Title
                ORDER BY be.Title";

        using var connection = await CreateConnectionAsync();
        return await GetBookEditionsWithRelations(connection, sql, new { Title = $"%{title}%" });
    }

    private async Task<List<BookEdition>> GetPagedResults(string sql, int pageNumber, int pageSize)
    {
        using var connection = await CreateConnectionAsync();
        return await GetBookEditionsWithRelations(connection, sql, new
        {
            Offset = (pageNumber - 1) * pageSize,
            PageSize = pageSize
        });
    }

    private async Task<BookEdition?> GetBookEditionWithRelations(string sql, object parameters)
    {
        using var connection = await CreateConnectionAsync();
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
                        bookEditionEntry.Book = bookEntry;
                    }

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
            param: parameters,
            splitOn: "BookID,PublisherID,LanguageCode,TranslatorID,AuthorID,GenreID");

        return bookEditionDictionary.Values.FirstOrDefault();
    }

    private async Task<List<BookEdition>> GetBookEditionsWithRelations(IDbConnection connection, string sql, object param)
    {
        var bookEditionDictionary = new Dictionary<int, BookEdition>();
        var bookDictionary = new Dictionary<int, Book>();

        await connection.QueryAsync<BookEdition, Book, Publisher, Translator, Author, Genre, BookEdition>(
            sql,
            (bookEdition, book, publisher, translator, author, genre) =>
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

                return bookEditionEntry;
            },
            param,
            splitOn: "BookID,PublisherID,TranslatorID,AuthorID,GenreID");

        return bookEditionDictionary.Values.ToList();
    }
}
