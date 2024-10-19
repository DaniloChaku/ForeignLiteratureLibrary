using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Interfaces;
using Microsoft.Data.SqlClient;
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
                 ISBN, EditionTitle, LanguageID, PageCount, 
                 ShelfLocation, TotalCopies, 
                 BookID, PublisherID, EditionPublicationYear)
             OUTPUT INSERTED.BookEditionID
             VALUES (
                 @ISBN, @EditionTitle, @LanguageID, @PageCount,
                 @ShelfLocation, @TotalCopies,
                 @BookID, @PublisherID, @EditionPublicationYear)";

            bookEdition.BookEditionID = await connection.QuerySingleAsync<int>(
                sql, bookEdition, transaction);

            await UpdateTranslatorsAsync(bookEdition, connection, transaction);

            transaction.Commit();
        }
        catch (SqlException ex) when (ex.Number == 50000 && ex.Message.Contains("less than open loans"))
        {
            throw new BookEditionUnavailableException(ex.Message, ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_BookEdition_ISBN"))
        {
            throw new CheckConstraintViolationException(
                "Cannot add the book edition because the ISBN cannot be empty", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_BookEdition_EditionTitle"))
        {
            throw new CheckConstraintViolationException(
                "Cannot add the book edition because the title cannot be empty", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_BookEdition_TotalCopies"))
        {
            throw new CheckConstraintViolationException(
                $"Cannot add the book edition because the total copies '{bookEdition.TotalCopies}' must be greater than or equal to 0", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_BookEdition_PageCount"))
        {
            throw new CheckConstraintViolationException(
                $"Cannot add the book edition because the page count '{bookEdition.PageCount}' must be greater than 0", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_BookEdition_EditionPublicationYear"))
        {
            throw new CheckConstraintViolationException(
                $"Cannot add the book edition because the publication year '{bookEdition.EditionPublicationYear}' must be greater than 0", ex);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            HandleForeignKeyConstraintViolation(ex, bookEdition);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                $"Cannot add the book edition because the ISBN '{bookEdition.ISBN}' already exists", ex);
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot add the book edition because a required field is missing", ex);
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
                 EditionTitle = @EditionTitle,
                 LanguageID = @LanguageID,
                 PageCount = @PageCount,
                 ShelfLocation = @ShelfLocation,
                 TotalCopies = @TotalCopies,
                 PublisherID = @PublisherID,
                 EditionPublicationYear = @EditionPublicationYear
             WHERE BookEditionID = @BookEditionID";

            await connection.ExecuteAsync(sql, bookEdition, transaction);
            await UpdateTranslatorsAsync(bookEdition, connection, transaction);

            transaction.Commit();
        }
        catch (SqlException ex) when (ex.Number == 50000 && ex.Message.Contains("less than open loans"))
        {
            throw new BookEditionUnavailableException(ex.Message, ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_BookEdition_ISBN"))
        {
            throw new CheckConstraintViolationException(
                "Cannot update the book edition because the ISBN cannot be empty", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_BookEdition_EditionTitle"))
        {
            throw new CheckConstraintViolationException(
                "Cannot update the book edition because the title cannot be empty", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_BookEdition_TotalCopies"))
        {
            throw new CheckConstraintViolationException(
                $"Cannot update the book edition because the total copies '{bookEdition.TotalCopies}' must be greater than or equal to 0", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_BookEdition_PageCount"))
        {
            throw new CheckConstraintViolationException(
                $"Cannot update the book edition because the page count '{bookEdition.PageCount}' must be greater than 0", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_BookEdition_EditionPublicationYear"))
        {
            throw new CheckConstraintViolationException(
                $"Cannot update the book edition because the publication year '{bookEdition.EditionPublicationYear}' must be greater than 0", ex);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            HandleForeignKeyConstraintViolation(ex, bookEdition);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                $"Cannot update the book edition because the ISBN '{bookEdition.ISBN}' already exists", ex);
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot update the book edition because a required field is missing", ex);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private void HandleForeignKeyConstraintViolation(SqlException ex, BookEdition bookEdition)
    {
        if (ex.Message.Contains("FK_BookEdition_Book", StringComparison.InvariantCultureIgnoreCase))
        {
            throw new ForeignKeyViolationException(
                $"Cannot add or update the book edition because the book '{bookEdition.BookID}' does not exist", ex);
        }
        if (ex.Message.Contains("FK_BookEdition_Language", StringComparison.InvariantCultureIgnoreCase))
        {
            throw new ForeignKeyViolationException(
                $"Cannot add or update the book edition because the language '{bookEdition.LanguageID}' does not exist", ex);
        }
        if (ex.Message.Contains("FK_BookEdition_Publisher", StringComparison.InvariantCultureIgnoreCase))
        {
            throw new ForeignKeyViolationException(
                $"Cannot add or update the book edition because the publisher '{bookEdition.PublisherID}' does not exist", ex);
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
        try
        {
            using var connection = await CreateConnectionAsync();
            const string sql = "DELETE FROM BookEdition WHERE BookEditionId = @BookEditionId";
            await connection.ExecuteAsync(sql, new { BookEditionId = bookEditionId });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new ForeignKeyViolationException(
                $"Cannot delete the book edition '{bookEditionId}' because it is referenced by other entities.", ex);
        }
    }
}
