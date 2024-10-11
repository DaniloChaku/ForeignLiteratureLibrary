using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Services;

public class PublisherService : IPublisherService
{
    private readonly IPublisherRepository _publisherRepository;

    public PublisherService(IPublisherRepository publisherRepository)
    {
        _publisherRepository = publisherRepository;
    }

    public async Task AddPublisherAsync(PublisherDto publisherDto)
    {
        var publisher = publisherDto.ToEntity();
        await _publisherRepository.AddAsync(publisher);
    }

    public async Task UpdatePublisherAsync(PublisherDto publisherDto)
    {
        var publisher = publisherDto.ToEntity();
        await _publisherRepository.UpdateAsync(publisher);
    }

    public async Task DeletePublisherAsync(int publisherId)
    {
        await _publisherRepository.DeleteAsync(publisherId);
    }

    public async Task<PublisherDto?> GetPublisherByIdAsync(int publisherId)
    {
        var publisher = await _publisherRepository.GetByIdAsync(publisherId);
        return publisher?.ToDto();
    }

    public async Task<PaginatedResult<PublisherDto>> GetPublishersPageAsync(int pageNumber, int pageSize)
    {
        var publishers = await _publisherRepository.GetPageAsync(pageNumber, pageSize);
        var totalItems = await _publisherRepository.GetCountAsync();

        return new PaginatedResult<PublisherDto>
        {
            Items = publishers.ConvertAll(p => p.ToDto()),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<List<PublisherDto>> GetAllPublishersAsync()
    {
        var publishers = await _publisherRepository.GetAllAsync();
        return publishers.ConvertAll(p => p.ToDto());
    }
}
