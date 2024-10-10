using ForeignLiteratureLibrary.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ForeignLiteratureLibrary.BLL.Dtos;

public class BookDto
{
    public int BookID { get; set; }

    [Required]
    [StringLength(255, ErrorMessage = "Original Title cannot exceed 255 characters.")]
    public string OriginalTitle { get; set; } = string.Empty;

    [Required]
    [StringLength(3, MinimumLength = 2, ErrorMessage = "Original Language Code must be between 2 and 3 characters long.")]
    public string OriginalLanguageCode { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "PublicationYear must be a positive number.")]
    public int PublicationYear { get; set; }

    public LanguageDto? OriginalLanguage { get; set; }

    public List<AuthorDto> Authors { get; set; } = [];

    public List<GenreDto> Genres { get; set; } = [];

    public List<BookEditionDto> Editions { get; set; } = [];

    public Book ToEntity()
    {
        return new Book
        {
            BookID = this.BookID,
            OriginalTitle = this.OriginalTitle.Trim(),
            OriginalLanguageCode = this.OriginalLanguageCode.Trim(),
            PublicationYear = this.PublicationYear
        };
    }
}

public static class BookExtensions
{
    public static BookDto ToDto(this Book book)
    {
        return new BookDto
        {
            BookID = book.BookID,
            OriginalTitle = book.OriginalTitle.Trim(),
            OriginalLanguageCode = book.OriginalLanguageCode.Trim(),
            PublicationYear = book.PublicationYear,
            OriginalLanguage = book.OriginalLanguage?.ToDto(),
            Authors = book.Authors.Select(a => a.ToDto()).ToList(),
            Genres = book.Genres.Select(g => g.ToDto()).ToList(),
            Editions = book.Editions.Select(e => e.ToDto()).ToList()
        };
    }
}
