using Training.Application.Events.DTOs;
using Training.Application.Events.Interfaces;


namespace Training.Application.Events.UseCases;

public class UpdateEventUseCase
{
    private readonly IEventRepository _repository;

    public UpdateEventUseCase(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task Execute(Guid id, UpdateEventDto dto)
    {
        var ev = await _repository.GetByIdAsync(id);

        if (ev == null)
            throw new KeyNotFoundException("Event not found");

        ev.Update(dto.Titre, dto.Description, dto.Date, dto.Capacite);

        await _repository.UpdateAsync(ev);
    }
}