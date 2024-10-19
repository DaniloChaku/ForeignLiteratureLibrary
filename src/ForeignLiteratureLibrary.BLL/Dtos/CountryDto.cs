using ForeignLiteratureLibrary.DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace ForeignLiteratureLibrary.BLL.Dtos;

public class CountryDto
{
    public int CountryID { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    public Country ToEntity()
    {
        return new Country
        {
            CountryID = this.CountryID,
            CountryName = this.Name.Trim()
        };
    }
}

public static class CountryExtensions
{
    public static CountryDto ToDto(this Country country)
    {
        return new CountryDto
        {
            CountryID = country.CountryID,
            Name = country.CountryName.Trim()
        };
    }
}

