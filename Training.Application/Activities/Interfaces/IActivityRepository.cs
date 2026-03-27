using Training.Domain.Entities;

namespace Training.Application.Activities.Interfaces;

public interface IActivityRepository
{
    Task<ActivitySportive?> GetByIdAsync(Guid id);

    Task<IEnumerable<ActivitySportive>> GetAllAsync();

    Task AddAsync(ActivitySportive activity);

    Task UpdateAsync(ActivitySportive activity);
}