using ForeignLiteratureLibrary.DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace ForeignLiteratureLibrary.BLL.Dtos;

public class PublisherDto
{
    public int PublisherID { get; set; }

    [Required]
    [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters.")]
    public string Name { get; set; } = string.Empty;

    public Publisher ToEntity()
    {
        return new Publisher
        {
            PublisherID = this.PublisherID,
            Name = this.Name.Trim()
        };
    }
}

public static class PublisherExtensions
{
    public static PublisherDto ToDto(this Publisher publisher)
    {
        return new PublisherDto
        {
            PublisherID = publisher.PublisherID,
            Name = publisher.Name.Trim()
        };
    }
}

