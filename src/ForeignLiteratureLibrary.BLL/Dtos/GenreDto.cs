using ForeignLiteratureLibrary.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ForeignLiteratureLibrary.BLL.Dtos;

public class GenreDto
{
    public int GenreID { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
    public string Name { get; set; } = string.Empty;

    public List<BookDto> Books { get; set; } = [];

    public Genre ToEntity()
    {
        return new Genre
        {
            GenreID = this.GenreID,
            Name = this.Name.Trim(),
            Books = this.Books.ConvertAll(b => b.ToEntity())
        };
    }
}

public static class GenreExtensions
{
    public static GenreDto ToDto(this Genre genre)
    {
        return new GenreDto
        {
            GenreID = genre.GenreID,
            Name = genre.Name.Trim(),
            Books = genre.Books.Select(b => b.ToDto()).ToList()
        };
    }
}