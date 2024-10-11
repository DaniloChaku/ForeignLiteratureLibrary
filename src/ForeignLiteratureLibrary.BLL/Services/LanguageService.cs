using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Services;

public class LanguageService : ILanguageService
{
    private readonly ILanguageRepository _languageRepository;

    public LanguageService(ILanguageRepository languageRepository)
    {
        _languageRepository = languageRepository;
    }

    public async Task AddLanguageAsync(LanguageDto languageDto)
    {
        var language = languageDto.ToEntity();
        await _languageRepository.AddAsync(language);
    }

    public async Task UpdateLanguageAsync(LanguageDto languageDto)
    {
        var language = languageDto.ToEntity();
        await _languageRepository.UpdateAsync(language);
    }

    public async Task DeleteLanguageAsync(string languageCode)
    {
        await _languageRepository.DeleteAsync(languageCode);
    }

    public async Task<LanguageDto?> GetLanguageByCodeAsync(string languageCode)
    {
        var language = await _languageRepository.GetByCodeAsync(languageCode);
        return language?.ToDto();
    }

    public async Task<PaginatedResult<LanguageDto>> GetLanguagesPageAsync(int pageNumber, int pageSize)
    {
        var languages = await _languageRepository.GetPageAsync(pageNumber, pageSize);
        var totalItems = await _languageRepository.GetCountAsync();

        return new PaginatedResult<LanguageDto>
        {
            Items = languages.ConvertAll(l => l.ToDto()),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<List<LanguageDto>> GetAllLanguagesAsync()
    {
        var languages = await _languageRepository.GetAllAsync();
        return languages.ConvertAll(l => l.ToDto());
    }
}
