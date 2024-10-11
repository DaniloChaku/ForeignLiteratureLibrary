using ForeignLiteratureLibrary.BLL.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Interfaces;

public interface IGenreService
{
    Task AddGenreAsync(GenreDto genreDto);

    Task UpdateGenreAsync(GenreDto genreDto);

    Task DeleteGenreAsync(int genreId);

    Task<GenreDto?> GetGenreByIdAsync(int genreId);

    Task<PaginatedResult<GenreDto>> GetGenresPageAsync(int pageNumber, int pageSize);

    Task<List<GenreDto>> GetAllGenresAsync();
}
