using Training.Application.Events.DTOs;
using Training.Application.Events.Interfaces;
using Training.Application.Exceptions;
using Training.Application.Spaces.Interfaces;
using Training.Domain.Entities;
using Training.Domain.Exceptions;

namespace Training.Application.Events.UseCases;

public class CreateEventUseCase
{
    private readonly IEventRepository _repository;
    private readonly ISpaceRepository _spaceRepository;

    public CreateEventUseCase(IEventRepository repository, ISpaceRepository spaceRepository)
    {
        _repository = repository;
        _spaceRepository = spaceRepository;
    }

    public async Task<Guid> Execute(CreateEventDto dto)
    {
        Space? space = await _spaceRepository.GetByIdAsync(dto.SpaceId);

        if (space == null)
            throw new SpaceNotFoundException(dto.SpaceId);

        if (!space.IsActive)
            throw new SpaceInactiveException();

        if (space.Capacity.HasValue && dto.Capacite > space.Capacity.Value)
            throw new InvalidEventCapacityException();

        bool available = await _spaceRepository.IsSpaceAvailableAsync(
            dto.SpaceId,
            dto.DateDebut,
            dto.DateFin,
            excludedSessionId: null,
            excludedEventId: null);

        if (!available)
            throw new SpaceUnavailableException();

        Event ev = new(dto.Titre, dto.Description, dto.DateDebut, dto.DateFin, dto.Capacite, dto.SpaceId);

        await _repository.AddAsync(ev);

        return ev.Id;
    }
}
