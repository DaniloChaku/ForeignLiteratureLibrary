using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.Web.Models;

public class TopBooksAndAuthorsViewModel
{
    public List<TopAuthor> TopAuthors { get; set; } = [];

    public List<TopBook> TopBooks { get; set; } = [];
}
