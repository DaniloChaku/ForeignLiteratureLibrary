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
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(3, MinimumLength = 2, ErrorMessage = "CountryCode must be 2 or 3 characters long.")]
    public string CountryCode { get; set; } = string.Empty;

    public CountryDto? Country { get; set; }
    public List<BookDto> Books { get; set; } = [];

    public Author ToEntity()
    {
        return new Author
        {
            AuthorID = this.AuthorID,
            FullName = this.FullName.Trim(),
            CountryCode = this.CountryCode.Trim(),
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
            FullName = author.FullName.Trim(),
            CountryCode = author.CountryCode.Trim(),
            Country = author.Country?.ToDto(),
            Books = author.Books.Select(b => b.ToDto()).ToList()
        };
    }
}