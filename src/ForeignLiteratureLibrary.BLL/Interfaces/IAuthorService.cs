using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Interfaces;

public interface IAuthorService
{
    Task AddAuthorAsync(AuthorDto authorDto);

    Task UpdateAuthorAsync(AuthorDto authorDto);

    Task DeleteAuthorAsync(int authorId);

    Task<AuthorDto?> GetAuthorByIdAsync(int authorId);

    Task<PaginatedResult<AuthorDto>> GetAuthorsPageAsync(int pageNumber, int pageSize);

    Task<List<AuthorDto>> GetAllAuthorsAsync();

    Task<List<TopAuthor>> GetTop10AuthorsAsync(DateTime startDate, DateTime endDate);
}
