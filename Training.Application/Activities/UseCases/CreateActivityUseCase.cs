using Training.Application.Activities.DTOs;
using Training.Application.Activities.Interfaces;
using Training.Application.Activities.Mappers;

namespace Training.Application.Activities.UseCases
{




    namespace Training.Application.Activities.UseCases
    {
        public class CreateActivityUseCase
        {
            private readonly IActivityRepository _activityRepository;

            public CreateActivityUseCase(IActivityRepository activityRepository)
            {
                _activityRepository = activityRepository;
            }

            public async Task<ActivityResponse> ExecuteAsync(CreateActivityRequest request)
            {
                if (string.IsNullOrWhiteSpace(request.Nom))
                    throw new ArgumentException("Nom obligatoire");

                var activity = ActivityMapper.ToEntity(request);
                var createdActivity = await _activityRepository.AddAsync(activity);

                return ActivityMapper.ToResponse(createdActivity);
            }
        }
    }
}
