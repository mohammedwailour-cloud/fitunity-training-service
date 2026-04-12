using Training.Application.Coachs.DTOs;
using Training.Application.Coachs.Interfaces;
using Training.Application.Coachs.Mappers;

namespace Training.Application.Coachs.UseCases;

public class GetCoachesUseCase
{
    private readonly ICoachRepository _repository;

    public GetCoachesUseCase(ICoachRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CoachDto>> Execute(int page, int pageSize)
    {
        if (page <= 0)
            throw new ArgumentException("Page must be greater than 0");

        if (pageSize <= 0)
            throw new ArgumentException("PageSize must be greater than 0");

        var coaches = await _repository.GetAllAsync(page, pageSize);

        return CoachMapper.ToDtoList(coaches);
    }
}