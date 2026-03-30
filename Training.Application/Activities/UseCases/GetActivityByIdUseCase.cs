using Training.Application.Activities.DTOs;
using Training.Application.Activities.Interfaces;
using Training.Application.Activities.Mappers;



namespace Training.Application.Activities.UseCases
{       
        public class GetActivityByIdUseCase
        {
            private readonly IActivityRepository _activityRepository;

            public GetActivityByIdUseCase(IActivityRepository activityRepository)
            {
                _activityRepository = activityRepository;
            }

            public async Task<ActivityResponse?> ExecuteAsync(Guid id)
            {
                var activity = await _activityRepository.GetByIdAsync(id);

                if (activity == null)
                    return null;

                return ActivityMapper.ToResponse(activity);
            }
        }
    
}
