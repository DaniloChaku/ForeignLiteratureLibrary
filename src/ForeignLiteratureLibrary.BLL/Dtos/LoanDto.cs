using ForeignLiteratureLibrary.BLL.ValidationAttributes;
using ForeignLiteratureLibrary.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ForeignLiteratureLibrary.BLL.Dtos;

public class LoanDto
{
    public int LoanID { get; set; }

    public int BookEditionID { get; set; }

    public int ReaderID { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [DateLaterThan1900]
    public DateTime LoanDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [DateLaterThan1900]
    public DateTime DueDate { get; set; }

    [DataType(DataType.Date)]
    [DateLaterThan1900]
    public DateTime? ReturnDate { get; set; }

    public BookEditionDto? BookEdition { get; set; }

    public ReaderDto? Reader { get; set; }

    public Loan ToEntity()
    {
        return new Loan
        {
            LoanID = this.LoanID,
            BookEditionID = this.BookEditionID,
            ReaderID = this.ReaderID,
            LoanDate = this.LoanDate,
            DueDate = this.DueDate,
            ReturnDate = this.ReturnDate,
            BookEdition = this.BookEdition?.ToEntity(),
            Reader = this.Reader?.ToEntity()
        };
    }
}

public static class BookEditionLoanExtensions
{
    public static LoanDto ToDto(this Loan loan)
    {
        return new LoanDto
        {
            LoanID = loan.LoanID,
            BookEditionID = loan.BookEditionID,
            ReaderID = loan.ReaderID,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            BookEdition = loan.BookEdition?.ToDto(),
            Reader = loan.Reader?.ToDto()
        };
    }
}
