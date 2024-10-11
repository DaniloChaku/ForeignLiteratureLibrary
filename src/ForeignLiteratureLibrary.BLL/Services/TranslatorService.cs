using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Services;

public class TranslatorService : ITranslatorService
{
    private readonly ITranslatorRepository _translatorRepository;

    public TranslatorService(ITranslatorRepository translatorRepository)
    {
        _translatorRepository = translatorRepository;
    }

    public async Task AddTranslatorAsync(TranslatorDto translatorDto)
    {
        var translator = translatorDto.ToEntity();
        await _translatorRepository.AddAsync(translator);
    }

    public async Task UpdateTranslatorAsync(TranslatorDto translatorDto)
    {
        var translator = translatorDto.ToEntity();
        await _translatorRepository.UpdateAsync(translator);
    }

    public async Task DeleteTranslatorAsync(int translatorId)
    {
        await _translatorRepository.DeleteAsync(translatorId);
    }

    public async Task<TranslatorDto?> GetTranslatorByIdAsync(int translatorId)
    {
        var translator = await _translatorRepository.GetByIdAsync(translatorId);
        return translator?.ToDto();
    }

    public async Task<PaginatedResult<TranslatorDto>> GetTranslatorsPageAsync(int pageNumber, int pageSize)
    {
        var translators = await _translatorRepository.GetPageAsync(pageNumber, pageSize);
        var totalItems = await _translatorRepository.GetCountAsync();

        return new PaginatedResult<TranslatorDto>
        {
            Items = translators.ConvertAll(t => t.ToDto()),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<List<TranslatorDto>> GetAllTranslatorsAsync()
    {
        var translators = await _translatorRepository.GetAllAsync();
        return translators.ConvertAll(t => t.ToDto());
    }
}
