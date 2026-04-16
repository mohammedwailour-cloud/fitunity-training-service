using Microsoft.EntityFrameworkCore;
using Training.Application.Activities.Interfaces;
using Training.Domain.Entities;
using Training.Infrastructure.Persistence;

namespace Training.Infrastructure.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly TrainingDbContext _context;

        public ActivityRepository(TrainingDbContext context)
        {
            _context = context;
        }

        public async Task<ActivitySportive> AddAsync(ActivitySportive activity)
        {
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
            return activity;
        }

        public async Task<ActivitySportive?> GetByIdAsync(Guid id)
        {
            return await _context.Activities
                .Include(a => a.Sessions)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<ActivitySportive>> GetAllAsync()
        {
            return await _context.Activities.ToListAsync();
        }

        public async Task<IEnumerable<ActivitySportive>> GetByIdsAsync(IEnumerable<Guid> activityIds)
        {
            List<Guid> activityIdList = activityIds.Distinct().ToList();

            if (activityIdList.Count == 0)
            {
                return Array.Empty<ActivitySportive>();
            }

            return await _context.Activities
                .Where(activity => activityIdList.Contains(activity.Id))
                .ToListAsync();
        }

        public async Task UpdateAsync(ActivitySportive activity)
        {
            _context.Activities.Update(activity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            ActivitySportive? activity = await _context.Activities.FindAsync(id);
            if (activity != null)
            {
                _context.Activities.Remove(activity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(IEnumerable<ActivitySportive> Items, int TotalCount)>
            GetPagedAsync(int page, int pageSize)
        {
            IQueryable<ActivitySportive> query = _context.Activities.AsQueryable();

            int totalCount = await query.CountAsync();

            List<ActivitySportive> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
