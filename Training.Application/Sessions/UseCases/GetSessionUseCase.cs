using Training.Application.Exceptions;
using Training.Application.Sessions.DTOs;
using Training.Application.Sessions.Interfaces;
using Training.Application.Sessions.Mappers;
using Training.Domain.Entities;

namespace Training.Application.Sessions.UseCases;

public class GetSessionUseCase
{
    private readonly ISessionRepository _sessionRepository;

    public GetSessionUseCase(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<SessionResponse?> Execute(Guid id)
    {
        Session? session = await _sessionRepository.GetByIdAsync(id);

        if (session == null)
            return null;

        if (session.Space == null)
            throw new SpaceNotFoundException(session.SpaceId);

        return SessionMapper.ToResponse(session, session.Space);
    }
}
