using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Services;

public class CountryService : ICountryService
{
    private readonly ICountryRepository _countryRepository;

    public CountryService(ICountryRepository countryRepository)
    {
        _countryRepository = countryRepository;
    }

    public async Task AddCountryAsync(CountryDto countryDto)
    {
        var country = countryDto.ToEntity();
        await _countryRepository.AddAsync(country);
    }

    public async Task UpdateCountryAsync(CountryDto countryDto)
    {
        var country = countryDto.ToEntity();
        await _countryRepository.UpdateAsync(country);
    }

    public async Task DeleteCountryAsync(string countryCode)
    {
        await _countryRepository.DeleteAsync(countryCode);
    }

    public async Task<CountryDto?> GetCountryByCodeAsync(string countryCode)
    {
        var country = await _countryRepository.GetByCodeAsync(countryCode);
        return country?.ToDto();
    }

    public async Task<PaginatedResult<CountryDto>> GetCountriesPageAsync(int pageNumber, int pageSize)
    {
        var countries = await _countryRepository.GetPageAsync(pageNumber, pageSize);
        var totalItems = await _countryRepository.GetCountAsync();

        return new PaginatedResult<CountryDto>
        {
            Items = countries.ConvertAll(c => c.ToDto()),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<List<CountryDto>> GetAllCountriesAsync()
    {
        var countries = await _countryRepository.GetAllAsync();
        return countries.ConvertAll(c => c.ToDto());
    }
}

