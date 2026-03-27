using Training.Application.Sessions.DTOs;
using Training.Application.Sessions.Interfaces;
using Training.Application.Sessions.Mappers;

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
        var session = await _sessionRepository.GetByIdAsync(id);

        if (session == null)
            return null;

        return SessionMapper.ToResponse(session);
    }
}