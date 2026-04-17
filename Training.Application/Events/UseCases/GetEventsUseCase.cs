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

    public async Task<List<EventDto>> Execute(int page, int pageSize)
    {
        List<Event> events = await _repository.GetAllAsync(page, pageSize);

        return EventMapper.ToDtoList(events);
    }
}
