using ForeignLiteratureLibrary.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ForeignLiteratureLibrary.BLL.Dtos;

public class AuthorDto
{
    public int AuthorID { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
    public string AuthorFullName { get; set; } = string.Empty;

    public int CountryID { get; set; }

    [Range(0, 9999, ErrorMessage = "BirthYear must be between 0 and 9999.")]
    public int? BirthYear { get; set; }

    [Range(0, 9999, ErrorMessage = "DeathYear must be between 0 and 9999.")]
    public int? DeathYear { get; set; }

    public CountryDto? Country { get; set; }

    public List<BookDto> Books { get; set; } = [];

    public Author ToEntity()
    {
        return new Author
        {
            AuthorID = this.AuthorID,
            AuthorFullName = this.AuthorFullName.Trim(),
            CountryID = this.CountryID,
            BirthYear = this.BirthYear,
            DeathYear = this.DeathYear,
            Books = this.Books.ConvertAll(b => b.ToEntity())
        };
    }
}

public static class AuthorExtensions
{
    public static AuthorDto ToDto(this Author author)
    {
        return new AuthorDto
        {
            AuthorID = author.AuthorID,
            AuthorFullName = author.AuthorFullName.Trim(),
            BirthYear = author.BirthYear,
            DeathYear = author.DeathYear,
            CountryID = author.CountryID,
            Country = author.Country?.ToDto(),
            Books = author.Books.Select(b => b.ToDto()).ToList()
        };
    }
}