using Microsoft.EntityFrameworkCore;
using Training.Application.Events.Interfaces;
using Training.Domain.Entities;
using Training.Infrastructure.Persistence;

namespace Training.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly TrainingDbContext _context;

        public EventRepository(TrainingDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Event ev)
        {
            await _context.Events.AddAsync(ev);
            await _context.SaveChangesAsync();
        }

        public async Task<Event?> GetByIdAsync(Guid id)
        {
            return await _context.Events
                .Include(ev => ev.Space)
                .FirstOrDefaultAsync(ev => ev.Id == id);
        }

        public async Task<(int TotalCount, List<Event> Items)> GetAllAsync(int page, int pageSize)
        {
            int totalCount = await _context.Events.CountAsync();

            List<Event> items = await _context.Events
                .Include(ev => ev.Space)
                .OrderBy(ev => ev.DateDebut)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (totalCount, items);
        }

        public async Task UpdateAsync(Event ev)
        {
            _context.Events.Update(ev);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsSpaceAvailableAsync(Guid spaceId, DateTime start, DateTime end, Guid? excludedEventId = null)
        {
            IQueryable<Event> query = _context.Events.Where(ev => ev.SpaceId == spaceId);

            if (excludedEventId.HasValue)
            {
                query = query.Where(ev => ev.Id != excludedEventId.Value);
            }

            bool conflictExists = await query.AnyAsync(ev => ev.DateDebut < end && ev.DateFin > start);

            return !conflictExists;
        }

        public async Task DeleteAsync(Guid id)
        {
            await Task.CompletedTask;
            throw new NotSupportedException("Physical delete is not allowed. Use soft delete strategy.");
        }
    }
}
