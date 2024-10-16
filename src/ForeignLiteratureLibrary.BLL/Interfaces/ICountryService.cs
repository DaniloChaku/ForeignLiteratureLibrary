﻿using ForeignLiteratureLibrary.BLL.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Interfaces;

public interface ICountryService
{
    Task AddCountryAsync(CountryDto countryDto);

    Task UpdateCountryAsync(CountryDto countryDto);

    Task DeleteCountryAsync(string countryCode);

    Task<CountryDto?> GetCountryByCodeAsync(string countryCode);

    Task<PaginatedResult<CountryDto>> GetCountriesPageAsync(int pageNumber, int pageSize);

    Task<List<CountryDto>> GetAllCountriesAsync();
}
