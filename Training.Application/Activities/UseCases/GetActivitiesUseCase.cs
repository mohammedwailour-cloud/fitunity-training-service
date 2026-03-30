using Training.Application.Activities.DTOs;
using Training.Application.Activities.Interfaces;
using Training.Application.Activities.Mappers;
using Training.Application.Common.DTOs;

namespace Training.Application.Activities.UseCases
{
    public class GetActivitiesUseCase
    {
        private readonly IActivityRepository _activityRepository;

        public GetActivitiesUseCase(IActivityRepository activityRepository)
        {
            _activityRepository = activityRepository;
        }

        public async Task<PagedResult<ActivityResponse>> ExecuteAsync(int page, int pageSize)
        {
            var (items, totalCount) = await _activityRepository.GetPagedAsync(page, pageSize);

            var mappedItems = items.Select(ActivityMapper.ToResponse);

            return new PagedResult<ActivityResponse>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Data = mappedItems
            };
        }
    }
}