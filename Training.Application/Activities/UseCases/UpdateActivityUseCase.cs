using Training.Application.Activities.DTOs;
using Training.Application.Activities.Interfaces;
using Training.Application.Activities.Mappers;

namespace Training.Application.Activities.UseCases
{
    public class UpdateActivityUseCase
    {
        private readonly IActivityRepository _activityRepository;

        public UpdateActivityUseCase(IActivityRepository activityRepository)
        {
            _activityRepository = activityRepository;
        }

        public async Task<ActivityResponse?> ExecuteAsync(Guid id, UpdateActivityRequest request)
        {
            var activity = await _activityRepository.GetByIdAsync(id);

            if (activity == null)
                return null;

            // Update entity (DDD)
            activity.Update(request.Nom, request.Description);

            await _activityRepository.UpdateAsync(activity);

            return ActivityMapper.ToResponse(activity);
        }
    }
}