using Training.Application.Common.DTOs;
using Training.Application.Spaces.DTOs;
using Training.Application.Spaces.Interfaces;
using Training.Application.Spaces.Mappers;
using Training.Domain.Entities;

namespace Training.Application.Spaces.UseCases;

public class GetSpacesUseCase
{
    private readonly ISpaceRepository _spaceRepository;

    public GetSpacesUseCase(ISpaceRepository spaceRepository)
    {
        _spaceRepository = spaceRepository;
    }

    public async Task<PagedResult<SpaceResponse>> ExecuteAsync(int page, int pageSize)
    {
        if (page <= 0)
        {
            page = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 10;
        }

        (IEnumerable<Space> items, int totalCount) = await _spaceRepository.GetPagedAsync(page, pageSize);
        IEnumerable<SpaceResponse> mappedItems = items.Select(SpaceMapper.ToResponse);

        return new PagedResult<SpaceResponse>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            Data = mappedItems
        };
    }
}
