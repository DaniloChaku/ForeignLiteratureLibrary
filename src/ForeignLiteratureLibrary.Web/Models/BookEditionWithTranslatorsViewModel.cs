using ForeignLiteratureLibrary.BLL.Dtos;

namespace ForeignLiteratureLibrary.Web.Models;

public class BookEditionWithTranslatorsViewModel
{
    public BookEditionDto BookEdition { get; set; } = new();
    public List<int>? Translators { get; set; } = [];
    
    public void PopulateBookEditionTranslators()
    {
        if (Translators != null)
        {
            BookEdition.Translators.AddRange(Translators.Select(id => new TranslatorDto() { TranslatorID = id }));
        }
    }
}
