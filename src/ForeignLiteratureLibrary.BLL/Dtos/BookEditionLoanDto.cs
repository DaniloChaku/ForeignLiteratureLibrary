using ForeignLiteratureLibrary.BLL.ValidationAttributes;
using ForeignLiteratureLibrary.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ForeignLiteratureLibrary.BLL.Dtos;

public class BookEditionLoanDto
{
    public int BookEditionLoanID { get; set; }

    public int BookEditionID { get; set; }

    [Required]
    [StringLength(20, ErrorMessage = "LibraryCardNumber cannot exceed 20 characters.")]
    public string LibraryCardNumber { get; set; } = string.Empty;

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

    public BookEditionLoan ToEntity()
    {
        return new BookEditionLoan
        {
            BookEditionLoanID = this.BookEditionLoanID,
            BookEditionID = this.BookEditionID,
            LibraryCardNumber = this.LibraryCardNumber.Trim(),
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
    public static BookEditionLoanDto ToDto(this BookEditionLoan loan)
    {
        return new BookEditionLoanDto
        {
            BookEditionLoanID = loan.BookEditionLoanID,
            BookEditionID = loan.BookEditionID,
            LibraryCardNumber = loan.LibraryCardNumber.Trim(),
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            BookEdition = loan.BookEdition?.ToDto(),
            Reader = loan.Reader?.ToDto()
        };
    }
}
