using Training.Domain.Entities;

namespace Training.Application.Activities.Interfaces;

public interface IActivityRepository
{
    Task<ActivitySportive> AddAsync(ActivitySportive activity);
    Task<ActivitySportive?> GetByIdAsync(Guid id);
    Task<IEnumerable<ActivitySportive>> GetAllAsync();
    Task<IEnumerable<ActivitySportive>> GetByIdsAsync(IEnumerable<Guid> activityIds);
    Task UpdateAsync(ActivitySportive activity);
    Task DeleteAsync(Guid id);

    Task<(IEnumerable<ActivitySportive> Items, int TotalCount)>
            GetPagedAsync(int page, int pageSize);
}
