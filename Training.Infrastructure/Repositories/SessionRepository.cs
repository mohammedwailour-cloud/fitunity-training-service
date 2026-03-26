using Training.Application.Interfaces;
using Training.Domain.Entities;
using Training.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class SessionRepository : ISessionRepository
{
    private readonly TrainingDbContext _context;

    public SessionRepository(TrainingDbContext context)
    {
        _context = context;
    }

    public async Task<Session?> GetByIdAsync(Guid id)
    {
        return await _context.Sessions
            .Include(s => s.Reservations)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Session>> GetAllAsync()
    {
        return await _context.Sessions
            .Include(s => s.Reservations)
            .ToListAsync();
    }


    public async Task AddAsync(Session session)
    {
        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Session session)
    {
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();
    }
    public async Task<(IEnumerable<Session>, int totalCount)> GetPagedAsync(int page, int pageSize)
    {
        var totalCount = await _context.Sessions.CountAsync();

        var skip = (page - 1) * pageSize;

        var sessions = await _context.Sessions
            .OrderBy(s => s.DateDebut)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return (sessions, totalCount);
    }
}