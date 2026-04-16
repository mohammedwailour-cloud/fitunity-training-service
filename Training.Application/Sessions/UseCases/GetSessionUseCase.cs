using Training.Application.Exceptions;
using Training.Application.Sessions.DTOs;
using Training.Application.Sessions.Interfaces;
using Training.Application.Sessions.Mappers;
using Training.Application.Spaces.Interfaces;
using Training.Domain.Entities;

namespace Training.Application.Sessions.UseCases;

public class GetSessionUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISpaceRepository _spaceRepository;

    public GetSessionUseCase(ISessionRepository sessionRepository, ISpaceRepository spaceRepository)
    {
        _sessionRepository = sessionRepository;
        _spaceRepository = spaceRepository;
    }

    public async Task<SessionResponse?> Execute(Guid id)
    {
        Session? session = await _sessionRepository.GetByIdAsync(id);

        if (session == null)
            return null;

        Space? space = await _spaceRepository.GetByIdAsync(session.SpaceId);

        if (space == null)
            throw new SpaceNotFoundException(session.SpaceId);

        return SessionMapper.ToResponse(session, space);
    }
}
