using ForeignLiteratureLibrary.DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace ForeignLiteratureLibrary.BLL.Dtos;

public class LanguageDto
{
    [Required]
    [StringLength(3, MinimumLength = 2, ErrorMessage = "LanguageCode must be 2 or 3 characters long.")]
    public string LanguageCode { get; set; } = string.Empty;

    [Required]
    [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
    public string Name { get; set; } = string.Empty;

    public Language ToEntity()
    {
        return new Language
        {
            LanguageCode = this.LanguageCode.Trim().ToLower(),
            Name = this.Name.Trim()
        };
    }
}

public static class LanguageExtensions
{
    public static LanguageDto ToDto(this Language language)
    {
        return new LanguageDto
        {
            LanguageCode = language.LanguageCode.Trim(),
            Name = language.Name.Trim()
        };
    }
}


