using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Interfaces;
using Microsoft.Data.SqlClient;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class BookEditionLoanRepository : BaseRepository, IBookEditionLoanRepository
{
    public BookEditionLoanRepository(string connectionString) : base(connectionString)
    {
    }

    private const string BaseSelectQuery = @"
            SELECT 
                bel.*, be.*, r.*
            FROM BookEditionLoan bel
            JOIN BookEdition be ON bel.BookEditionID = be.BookEditionID
            JOIN Reader r ON bel.LibraryCardNumber = r.LibraryCardNumber";

    public async Task AddAsync(BookEditionLoan loan)
    {
        try
        {
            const string sql = @"
                INSERT INTO BookEditionLoan 
                (BookEditionID, LibraryCardNumber, LoanDate, DueDate, ReturnDate)
                VALUES 
                (@BookEditionID, @LibraryCardNumber, @LoanDate, @DueDate, @ReturnDate);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            using var connection = await CreateConnectionAsync();
            loan.BookEditionLoanID = await connection.QuerySingleAsync<int>(sql, loan);
        }
        catch (SqlException ex) when (ex.Number == 50000 && ex.Message.Contains("No available copies"))
        {
            throw new BookEditionUnavailableException(ex.Message, ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("FK_BookLoan_BookEdition"))
        {
            throw new ForeignKeyViolationException(
                $"Cannot add the loan because the book edition with ID '{loan.BookEditionID}' does not exist", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("FK_BookLoan_Reader"))
        {
            throw new ForeignKeyViolationException(
                $"Cannot add the loan because the reader with library card number '{loan.LibraryCardNumber}' does not exist", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_DueDate"))
        {
            throw new CheckConstraintViolationException(
                "Cannot add the loan because the due date must be on or after the loan date", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_ReturnDate"))
        {
            throw new CheckConstraintViolationException(
                "Cannot add the loan because the return date must be on or after the loan date", ex);
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot add the loan because a required field is missing", ex);
        }
    }

    public async Task UpdateAsync(BookEditionLoan loan)
    {
        try
        {
            const string sql = @"
                UPDATE BookEditionLoan 
                SET BookEditionID = @BookEditionID,
                    LibraryCardNumber = @LibraryCardNumber,
                    LoanDate = @LoanDate,
                    DueDate = @DueDate,
                    ReturnDate = @ReturnDate
                WHERE BookEditionLoanID = @BookEditionLoanID";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, loan);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("FK_BookLoan_BookEdition"))
        {
            throw new ForeignKeyViolationException(
                $"Cannot update the loan because the book edition with ID '{loan.BookEditionID}' does not exist", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("FK_BookLoan_Reader"))
        {
            throw new ForeignKeyViolationException(
                $"Cannot update the loan because the reader with library card number '{loan.LibraryCardNumber}' does not exist", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_DueDate"))
        {
            throw new CheckConstraintViolationException(
                "Cannot update the loan because the due date must be on or after the loan date", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_ReturnDate"))
        {
            throw new CheckConstraintViolationException(
                "Cannot update the loan because the return date must be on or after the loan date", ex);
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot update the loan because a required field is missing", ex);
        }
    }

    public async Task DeleteAsync(int loanId)
    {
        try
        {
            const string sql = "DELETE FROM BookEditionLoan WHERE BookEditionLoanID = @LoanId";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, new { LoanId = loanId });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new ForeignKeyViolationException(
                $"Cannot delete the loan with ID '{loanId}' because it is referenced in other records.", ex);
        }
    }

    public async Task<BookEditionLoan?> GetByIdAsync(int loanId)
    {
        var sql = $"{BaseSelectQuery} WHERE bel.BookEditionLoanID = @LoanId";

        using var connection = await CreateConnectionAsync();
        var loans = await connection.QueryAsync<BookEditionLoan, BookEdition, Reader, BookEditionLoan>(
            sql,
            (loan, bookEdition, reader) =>
            {
                loan.BookEdition = bookEdition;
                loan.Reader = reader;
                return loan;
            },
            new { LoanId = loanId },
            splitOn: "ISBN,LibraryCardNumber"
        );

        return loans.FirstOrDefault();
    }

    public async Task<int> GetCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM BookEditionLoan";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<List<BookEditionLoan>> GetPageAsync(int pageNumber, int pageSize)
    {
        var sql = $@"{BaseSelectQuery}
                ORDER BY bel.LoanDate DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var loans = await connection.QueryAsync<BookEditionLoan, BookEdition, Reader, BookEditionLoan>(
            sql,
            (loan, bookEdition, reader) =>
            {
                loan.BookEdition = bookEdition;
                loan.Reader = reader;
                return loan;
            },
            new
            {
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            },
            splitOn: "ISBN,LibraryCardNumber"
        );

        return loans.ToList();
    }

    public async Task<List<BookEditionLoan>> GetOverduePageAsync(int pageNumber, int pageSize)
    {
        var sql = $@"{BaseSelectQuery}
                WHERE bel.ReturnDate IS NULL AND bel.DueDate < GETDATE()
                ORDER BY bel.DueDate
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var loans = await connection.QueryAsync<BookEditionLoan, BookEdition, Reader, BookEditionLoan>(
            sql,
            (loan, bookEdition, reader) =>
            {
                loan.BookEdition = bookEdition;
                loan.Reader = reader;
                return loan;
            },
            new
            {
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            },
            splitOn: "ISBN,LibraryCardNumber"
        );

        return loans.ToList();
    }
}
