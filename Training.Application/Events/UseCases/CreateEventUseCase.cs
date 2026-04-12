
using Training.Application.Events.Interfaces;
using Training.Domain.Entities;
using Training.Application.Events.DTOs;



namespace Training.Application.Events.UseCases;

public class CreateEventUseCase
{
    private readonly IEventRepository _repository;

    public CreateEventUseCase(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Execute(CreateEventDto dto)
    {
        var ev = new Event(dto.Titre, dto.Description, dto.Date, dto.Capacite);

        await _repository.AddAsync(ev);

        return ev.Id;
    }
}