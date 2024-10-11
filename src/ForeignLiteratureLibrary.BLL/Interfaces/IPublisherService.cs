using ForeignLiteratureLibrary.BLL.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Interfaces;

public interface IPublisherService
{
    Task AddPublisherAsync(PublisherDto publisherDto);

    Task UpdatePublisherAsync(PublisherDto publisherDto);

    Task DeletePublisherAsync(int publisherId);

    Task<PublisherDto?> GetPublisherByIdAsync(int publisherId);

    Task<PaginatedResult<PublisherDto>> GetPublishersPageAsync(int pageNumber, int pageSize);

    Task<List<PublisherDto>> GetAllPublishersAsync();
}
