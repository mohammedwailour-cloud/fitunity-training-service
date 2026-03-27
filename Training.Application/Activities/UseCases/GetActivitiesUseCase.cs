using Training.Application.Activities.Interfaces;
using Training.Domain.Entities;

namespace Training.Application.Activities.UseCases;

public class GetActivitiesUseCase
{
    private readonly IActivityRepository _activityRepository;

    public GetActivitiesUseCase(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<IEnumerable<ActivitySportive>> Execute()
    {
        return await _activityRepository.GetAllAsync();
    }
}