using ForeignLiteratureLibrary.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ForeignLiteratureLibrary.BLL.Dtos;

public class BookEditionDto
{
    public int BookEditionID { get; set; }

    [StringLength(17, ErrorMessage = "ISBN cannot exceed 17 characters.")]
    public string? ISBN { get; set; }

    [Required]
    [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(3, MinimumLength = 2, ErrorMessage = "LanguageCode must be 2 or 3 characters long.")]
    public string LanguageCode { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "PageCount must be greater than 0.")]
    public int PageCount { get; set; }

    [Required]
    [StringLength(60, ErrorMessage = "ShelfLocation cannot exceed 60 characters.")]
    public string ShelfLocation { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "TotalCopies must be at least 0.")]
    public int TotalCopies { get; set; }

    public int AvailableCopies { get; set; }

    public int BookID { get; set; }

    public int? PublisherID { get; set; }

    public BookDto? Book { get; set; }
    public LanguageDto? Language { get; set; }
    public PublisherDto? Publisher { get; set; }
    public List<TranslatorDto> Translators { get; set; } = [];
    public List<BookEditionLoanDto> Loans { get; set; } = [];

    public BookEdition ToEntity()
    {
        return new BookEdition
        {
            BookEditionID = this.BookEditionID,
            ISBN = this.ISBN?.Trim(),
            Title = this.Title.Trim(),
            LanguageCode = this.LanguageCode.Trim(),
            PageCount = this.PageCount,
            ShelfLocation = this.ShelfLocation.Trim(),
            TotalCopies = this.TotalCopies,
            AvailableCopies = this.AvailableCopies,
            BookID = this.BookID,
            PublisherID = this.PublisherID,
            Translators = this.Translators.ConvertAll(t => t.ToEntity()),
            Loans = this.Loans.ConvertAll(l => l.ToEntity())
        };
    }
}

public static class BookEditionExtensions
{
    public static BookEditionDto ToDto(this BookEdition bookEdition)
    {
        return new BookEditionDto
        {
            BookEditionID = bookEdition.BookEditionID,
            ISBN = bookEdition.ISBN?.Trim(),
            Title = bookEdition.Title.Trim(),
            LanguageCode = bookEdition.LanguageCode.Trim(),
            PageCount = bookEdition.PageCount,
            ShelfLocation = bookEdition.ShelfLocation.Trim(),
            TotalCopies = bookEdition.TotalCopies,
            AvailableCopies = bookEdition.AvailableCopies,
            BookID = bookEdition.BookID,
            PublisherID = bookEdition.PublisherID,
            Book = bookEdition.Book?.ToDto(),
            Language = bookEdition.Language?.ToDto(),
            Publisher = bookEdition.Publisher?.ToDto(),
            Translators = bookEdition.Translators.Select(t => t.ToDto()).ToList(),
            Loans = bookEdition.Loans.Select(l => l.ToDto()).ToList()
        };
    }
}