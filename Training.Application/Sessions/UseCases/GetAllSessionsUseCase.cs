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
        var sessions = await _repository.GetAllAsync();
        if (sessions == null)
            return null;

        return sessions
            .Select(SessionMapper.ToResponse)
            .ToList();
    }
}