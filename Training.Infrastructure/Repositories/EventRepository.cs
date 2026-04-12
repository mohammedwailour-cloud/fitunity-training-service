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
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Event>> GetAllAsync(int page, int pageSize)
        {
            return await _context.Events
                .OrderBy(e => e.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task UpdateAsync(Event ev)
        {
            _context.Events.Update(ev);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var ev = await _context.Events.FindAsync(id);

            if (ev == null)
                throw new KeyNotFoundException("Event not found");

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
        }
    }
}
