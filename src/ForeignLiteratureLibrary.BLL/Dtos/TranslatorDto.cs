using ForeignLiteratureLibrary.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ForeignLiteratureLibrary.BLL.Dtos;

public class TranslatorDto
{
    public int TranslatorID { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "FullName cannot exceed 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(3, MinimumLength = 2, ErrorMessage = "CountryCode must be between 2 and 3 characters long.")]
    public string CountryCode { get; set; } = string.Empty;

    public CountryDto? Country { get; set; }

    public List<BookEditionDto> BookEditions { get; set; } = [];

    public Translator ToEntity()
    {
        return new Translator
        {
            TranslatorID = this.TranslatorID,
            FullName = this.FullName.Trim(),
            CountryCode = this.CountryCode.Trim(),
            BookEditions = this.BookEditions.ConvertAll(be => be.ToEntity())
        };
    }
}

public static class TranslatorExtensions
{
    public static TranslatorDto ToDto(this Translator translator)
    {
        return new TranslatorDto
        {
            TranslatorID = translator.TranslatorID,
            FullName = translator.FullName.Trim(),
            CountryCode = translator.CountryCode.Trim(),
            Country = translator.Country?.ToDto(),
            BookEditions = translator.BookEditions.Select(be => be.ToDto()).ToList()
        };
    }
}