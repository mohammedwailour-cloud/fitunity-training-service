using Training.Application.Common.DTOs;
using Training.Application.Events.DTOs;
using Training.Application.Events.Interfaces;
using Training.Application.Events.Mappers;
using Training.Domain.Entities;

namespace Training.Application.Events.UseCases;

public class GetEventsUseCase
{
    private readonly IEventRepository _repository;

    public GetEventsUseCase(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<EventDto>> Execute(int page, int pageSize)
    {
        (int totalCount, List<Event> events) = await _repository.GetAllAsync(page, pageSize);
        List<EventDto> items = EventMapper.ToDtoList(events);

        return new PagedResult<EventDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0,
            Data = items
        };
    }
}
