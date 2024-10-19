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

    public int OriginalLanguageID { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "PublicationYear must be a positive number.")]
    public int PublicationYear { get; set; }

    public string? BookDescription { get; set; }

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
            OriginalLanguageID = this.OriginalLanguageID,
            FirstPublicationYear = this.PublicationYear,
            BookDescription = this.BookDescription?.Trim(),
            Authors = this.Authors.ConvertAll(a => a.ToEntity()),
            Genres = this.Genres.ConvertAll(g => g.ToEntity())
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
            OriginalLanguageID = book.OriginalLanguageID,
            PublicationYear = book.FirstPublicationYear,
            BookDescription = book.BookDescription?.Trim(),
            OriginalLanguage = book.OriginalLanguage?.ToDto(),
            Authors = book.Authors.Select(a => a.ToDto()).ToList(),
            Genres = book.Genres.Select(g => g.ToDto()).ToList(),
            Editions = book.Editions.Select(e => e.ToDto()).ToList()
        };
    }
}
