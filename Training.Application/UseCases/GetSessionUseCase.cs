using Training.Application.DTOs;
using Training.Application.Interfaces;
using Training.Application.Mappers;

namespace Training.Application.UseCases;

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