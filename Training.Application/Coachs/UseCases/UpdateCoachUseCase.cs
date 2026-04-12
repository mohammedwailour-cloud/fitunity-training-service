using Training.Application.Coachs.DTOs;
using Training.Application.Coachs.Interfaces;
using Training.Application.Coachs.Mappers;


namespace Training.Application.Coachs.UseCases;

public class UpdateCoachUseCase
{
    private readonly ICoachRepository _coachRepository;

    public UpdateCoachUseCase(ICoachRepository coachRepository)
    {
        _coachRepository = coachRepository;
    }

    public async Task<CoachDto?> ExecuteAsync(Guid id, UpdateCoachDto request)
    {
        var coach = await _coachRepository.GetByIdAsync(id);

        if (coach == null)
            return null;

        coach.Update(request.Nom, request.Email);

        await _coachRepository.UpdateAsync(coach);

        return CoachMapper.ToDto(coach);
    }
}