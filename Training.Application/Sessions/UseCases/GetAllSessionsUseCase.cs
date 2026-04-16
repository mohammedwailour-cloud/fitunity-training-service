using Training.Application.Sessions.DTOs;
using Training.Application.Sessions.Interfaces;
using Training.Application.Sessions.Mappers;
using Training.Domain.Entities;

public class GetAllSessionsUseCase
{
    private readonly ISessionRepository _repository;

    public GetAllSessionsUseCase(ISessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<SessionResponse>> ExecuteAsync()
    {
        IEnumerable<Session> sessions = await _repository.GetAllAsync();

        return sessions
            .Select(session => SessionMapper.ToResponse(session, session.Space))
            .ToList();
    }
}
