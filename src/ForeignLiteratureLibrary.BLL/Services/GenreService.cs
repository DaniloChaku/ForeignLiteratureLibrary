using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Services;

public class GenreService : IGenreService
{
    private readonly IGenreRepository _genreRepository;

    public GenreService(IGenreRepository genreRepository)
    {
        _genreRepository = genreRepository;
    }

    public async Task AddGenreAsync(GenreDto genreDto)
    {
        var genre = genreDto.ToEntity();
        await _genreRepository.AddAsync(genre);
    }

    public async Task UpdateGenreAsync(GenreDto genreDto)
    {
        var genre = genreDto.ToEntity();
        await _genreRepository.UpdateAsync(genre);
    }

    public async Task DeleteGenreAsync(int genreId)
    {
        await _genreRepository.DeleteAsync(genreId);
    }

    public async Task<GenreDto?> GetGenreByIdAsync(int genreId)
    {
        var genre = await _genreRepository.GetByIdAsync(genreId);
        return genre?.ToDto();
    }

    public async Task<PaginatedResult<GenreDto>> GetGenresPageAsync(int pageNumber, int pageSize)
    {
        var genres = await _genreRepository.GetPageAsync(pageNumber, pageSize);
        var totalItems = await _genreRepository.GetCountAsync();

        return new PaginatedResult<GenreDto>
        {
            Items = genres.ConvertAll(g => g.ToDto()),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<List<GenreDto>> GetAllGenresAsync()
    {
        var genres = await _genreRepository.GetAllAsync();
        return genres.ConvertAll(g => g.ToDto());
    }
}
