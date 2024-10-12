using ForeignLiteratureLibrary.DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace ForeignLiteratureLibrary.BLL.Dtos;

public class CountryDto
{
    [Required]
    [StringLength(3, MinimumLength = 2, ErrorMessage = "CountryCode must be between 2 and 3 characters long.")]
    public string CountryCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    public Country ToEntity()
    {
        return new Country
        {
            CountryCode = this.CountryCode.Trim().ToUpper(),
            Name = this.Name.Trim()
        };
    }
}

public static class CountryExtensions
{
    public static CountryDto ToDto(this Country country)
    {
        return new CountryDto
        {
            CountryCode = country.CountryCode.Trim(),
            Name = country.Name.Trim()
        };
    }
}

