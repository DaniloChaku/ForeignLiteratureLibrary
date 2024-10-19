using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Interfaces;
using Microsoft.Data.SqlClient;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class LoanRepository : BaseRepository, ILoanRepository
{
    public LoanRepository(string connectionString) : base(connectionString)
    {
    }

    private const string BaseSelectQuery = @"
            SELECT 
                l.*, be.*, r.*
            FROM Loan l
            JOIN BookEdition be ON l.BookEditionID = be.BookEditionID
            JOIN Reader r ON l.ReaderID = r.ReaderID";

    public async Task AddAsync(Loan loan)
    {
        try
        {
            const string sql = @"
                INSERT INTO Loan 
                (BookEditionID, ReaderID, LoanDate, DueDate, ReturnDate)
                VALUES 
                (@BookEditionID, @ReaderID, @LoanDate, @DueDate, @ReturnDate);";

            using var connection = await CreateConnectionAsync();

            // the LoanID is output in the trigger
            loan.LoanID = await connection.QuerySingleAsync<int>(sql, loan);
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
                $"Cannot add the loan because the reader with library card number '{loan.ReaderID}' does not exist", ex);
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

    public async Task UpdateAsync(Loan loan)
    {
        try
        {
            const string sql = @"
                UPDATE Loan 
                SET BookEditionID = @BookEditionID,
                    ReaderID = @ReaderID,
                    LoanDate = @LoanDate,
                    DueDate = @DueDate,
                    ReturnDate = @ReturnDate
                WHERE LoanID = @LoanID";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, loan);
        }
        catch (SqlException ex) when (ex.Number == 50000 && ex.Message.Contains("No available copies"))
        {
            throw new BookEditionUnavailableException(ex.Message, ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("FK_BookLoan_BookEdition"))
        {
            throw new ForeignKeyViolationException(
                $"Cannot update the loan because the book edition with ID '{loan.BookEditionID}' does not exist", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("FK_BookLoan_Reader"))
        {
            throw new ForeignKeyViolationException(
                $"Cannot update the loan because the reader with library card number '{loan.ReaderID}' does not exist", ex);
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
            const string sql = "DELETE FROM Loan WHERE LoanID = @LoanId";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, new { LoanId = loanId });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new ForeignKeyViolationException(
                $"Cannot delete the loan with ID '{loanId}' because it is referenced in other records.", ex);
        }
    }

    public async Task<Loan?> GetByIdAsync(int loanId)
    {
        var sql = $"{BaseSelectQuery} WHERE l.LoanID = @LoanId";

        using var connection = await CreateConnectionAsync();
        var loans = await connection.QueryAsync<Loan, BookEdition, Reader, Loan>(
            sql,
            (loan, bookEdition, reader) =>
            {
                loan.BookEdition = bookEdition;
                loan.Reader = reader;
                return loan;
            },
            new { LoanId = loanId },
            splitOn: "BookEditionID,ReaderID"
        );

        return loans.FirstOrDefault();
    }

    public async Task<int> GetCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Loan";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<List<Loan>> GetPageAsync(int pageNumber, int pageSize)
    {
        var sql = $@"{BaseSelectQuery}
        ORDER BY l.LoanDate DESC
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var loans = await connection.QueryAsync<Loan, BookEdition, Reader, Loan>(
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
            splitOn: "BookEditionID,ReaderID"
        );

        return loans.ToList();
    }

    public async Task<List<Loan>> GetOverduePageAsync(int pageNumber, int pageSize)
    {
        var sql = $@"{BaseSelectQuery}
        WHERE l.ReturnDate IS NULL AND l.DueDate < GETDATE()
        ORDER BY l.DueDate
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var loans = await connection.QueryAsync<Loan, BookEdition, Reader, Loan>(
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
            splitOn: "BookEditionID,ReaderID"
        );

        return loans.ToList();
    }
}
