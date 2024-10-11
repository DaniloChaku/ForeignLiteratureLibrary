using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Services;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _authorRepository;

    public AuthorService(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task AddAuthorAsync(AuthorDto authorDto)
    {
        var author = authorDto.ToEntity();
        await _authorRepository.AddAsync(author);
    }

    public async Task UpdateAuthorAsync(AuthorDto authorDto)
    {
        var author = authorDto.ToEntity();
        await _authorRepository.UpdateAsync(author);
    }

    public async Task DeleteAuthorAsync(int authorId)
    {
        await _authorRepository.DeleteAsync(authorId);
    }

    public async Task<AuthorDto?> GetAuthorByIdAsync(int authorId)
    {
        var author = await _authorRepository.GetByIdAsync(authorId);
        return author?.ToDto();
    }

    public async Task<PaginatedResult<AuthorDto>> GetAuthorsPageAsync(int pageNumber, int pageSize)
    {
        var authors = await _authorRepository.GetPageAsync(pageNumber, pageSize);
        var totalItems = await _authorRepository.GetCountAsync();

        return new PaginatedResult<AuthorDto>
        {
            Items = authors.ConvertAll(a => a.ToDto()),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<List<AuthorDto>> GetAllAuthorsAsync()
    {
        var authors = await _authorRepository.GetAllAsync();
        return authors.ConvertAll(a => a.ToDto());
    }

    public async Task<List<TopAuthor>> GetTop10AuthorsAsync(DateTime startDate, DateTime endDate)
    {
        return await _authorRepository.GetTop10AuthorsAsync(startDate, endDate);
    }
}
