using ForeignLiteratureLibrary.DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace ForeignLiteratureLibrary.BLL.Dtos;

public class LanguageDto
{
    public int LanguageID { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
    public string Name { get; set; } = string.Empty;

    public Language ToEntity()
    {
        return new Language
        {
            LanguageID = this.LanguageID,
            LanguageName = this.Name.Trim()
        };
    }
}

public static class LanguageExtensions
{
    public static LanguageDto ToDto(this Language language)
    {
        return new LanguageDto
        {
            LanguageID = language.LanguageID,
            Name = language.LanguageName.Trim()
        };
    }
}


