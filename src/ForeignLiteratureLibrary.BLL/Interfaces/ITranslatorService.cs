using ForeignLiteratureLibrary.BLL.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Interfaces;

public interface ITranslatorService
{
    Task AddTranslatorAsync(TranslatorDto translatorDto);

    Task UpdateTranslatorAsync(TranslatorDto translatorDto);

    Task DeleteTranslatorAsync(int translatorId);

    Task<TranslatorDto?> GetTranslatorByIdAsync(int translatorId);

    Task<PaginatedResult<TranslatorDto>> GetTranslatorsPageAsync(int pageNumber, int pageSize);

    Task<List<TranslatorDto>> GetAllTranslatorsAsync();
}
