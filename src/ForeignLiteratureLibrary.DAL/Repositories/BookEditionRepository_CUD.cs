using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Interfaces;
using System.Data;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public partial class BookEditionRepository : BaseRepository, IBookEditionRepository
{
    public BookEditionRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task AddAsync(BookEdition bookEdition)
    {
        using var connection = await CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string sql = @"
                    INSERT INTO BookEdition (
                        ISBN, Title, LanguageCode, PageCount, 
                        ShelfLocation, TotalCopies, AvailableCopies, 
                        BookId, PublisherId)
                    OUTPUT INSERTED.BookEditionId
                    VALUES (
                        @ISBN, @Title, @LanguageCode, @PageCount,
                        @ShelfLocation, @TotalCopies, @AvailableCopies,
                        @BookId, @PublisherId)";

            bookEdition.BookEditionID = await connection.QuerySingleAsync<int>(
                sql, bookEdition, transaction);

            await UpdateTranslatorsAsync(bookEdition, connection, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task UpdateAsync(BookEdition bookEdition)
    {
        using var connection = await CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string sql = @"
                    UPDATE BookEdition 
                    SET ISBN = @ISBN,
                        Title = @Title,
                        LanguageCode = @LanguageCode,
                        PageCount = @PageCount,
                        ShelfLocation = @ShelfLocation,
                        TotalCopies = @TotalCopies,
                        AvailableCopies = @AvailableCopies,
                        PublisherId = @PublisherId
                    WHERE BookEditionId = @BookEditionId";

            await connection.ExecuteAsync(sql, bookEdition, transaction);
            await UpdateTranslatorsAsync(bookEdition, connection, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private async Task UpdateTranslatorsAsync(
        BookEdition bookEdition,
        IDbConnection connection,
        IDbTransaction transaction)
    {
        // Remove existing relationships
        await connection.ExecuteAsync(
            "DELETE FROM BookEditionTranslator WHERE BookEditionId = @BookEditionId",
            new { bookEdition.BookEditionID },
            transaction);

        // Add new relationships
        if (bookEdition.Translators.Count != 0)
        {
            const string insertSql = @"
                    INSERT INTO BookEditionTranslator (BookEditionId, TranslatorId)
                    VALUES (@BookEditionId, @TranslatorId)";

            foreach (var translator in bookEdition.Translators)
            {
                await connection.ExecuteAsync(insertSql,
                    new
                    {
                        bookEdition.BookEditionID,
                        translator.TranslatorID
                    },
                    transaction);
            }
        }
    }

    public async Task DeleteAsync(int bookEditionId)
    {
        using var connection = await CreateConnectionAsync();
        const string sql = "DELETE FROM BookEdition WHERE BookEditionId = @BookEditionId";
        await connection.ExecuteAsync(sql, new { BookEditionId = bookEditionId });
    }
}
