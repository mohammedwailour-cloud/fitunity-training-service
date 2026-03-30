using Training.Application.Activities.DTOs;
using Training.Domain.Entities;

namespace Training.Application.Activities.Mappers
{
    public static class ActivityMapper
    {
        public static ActivitySportive ToEntity(CreateActivityRequest request)
        {
            return new ActivitySportive(
                request.Nom,
                request.Description
            );
        }

        public static ActivityResponse ToResponse(ActivitySportive activity)
        {
            return new ActivityResponse
            {
                Id = activity.Id,
                Name = activity.Nom,
                Description = activity.Description
            };
        }
    }
}
