using ForeignLiteratureLibrary.BLL.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Interfaces;

public interface ILanguageService
{
    Task AddLanguageAsync(LanguageDto languageDto);

    Task UpdateLanguageAsync(LanguageDto languageDto);

    Task DeleteLanguageAsync(int languageId);

    Task<LanguageDto?> GetLanguageByIdAsync(int languageId);

    Task<PaginatedResult<LanguageDto>> GetLanguagesPageAsync(int pageNumber, int pageSize);

    Task<List<LanguageDto>> GetAllLanguagesAsync();
}
