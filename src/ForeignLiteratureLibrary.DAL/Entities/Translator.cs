namespace ForeignLiteratureLibrary.DAL.Entities;

public class Translator
{
    public int TranslatorID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CountryOfOrigin { get; set; } = string.Empty;
    public Country? Country { get; set; }
}
