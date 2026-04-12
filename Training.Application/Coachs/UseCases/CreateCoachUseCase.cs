using Training.Application.Coachs.DTOs;
using Training.Application.Coachs.Interfaces;
using Training.Domain.Entities;

namespace Training.Application.Coachs.UseCases;

public class CreateCoachUseCase
{
    private readonly ICoachRepository _repository;

    public CreateCoachUseCase(ICoachRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Execute(CreateCoachDto dto)
    {
        var coach = new Coach(dto.Nom, dto.Email, dto.ActivityId);

        await _repository.AddAsync(coach);

        return coach.Id;
    }
}