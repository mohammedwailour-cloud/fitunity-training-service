using Training.Domain.Entities;
using Training.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Training.Application.Sessions.Interfaces;

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
            .Include(s => s.Space)
            .Include(s => s.Activity)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Session>> GetAllAsync()
    {
        return await _context.Sessions
            .Include(s => s.Reservations)
            .Include(s => s.Space)
            .Include(s => s.Activity)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetByIdsAsync(IEnumerable<Guid> sessionIds)
    {
        List<Guid> sessionIdList = sessionIds.Distinct().ToList();

        if (sessionIdList.Count == 0)
        {
            return Array.Empty<Session>();
        }

        return await _context.Sessions
            .Where(session => sessionIdList.Contains(session.Id))
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
        int totalCount = await _context.Sessions.CountAsync();

        int skip = (page - 1) * pageSize;

        List<Session> sessions = await _context.Sessions
            .Include(s => s.Space)
            .Include(s => s.Activity)
            .OrderBy(session => session.DateDebut)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return (sessions, totalCount);
    }

    public async Task<bool> IsSpaceAvailableAsync(Guid spaceId, DateTime start, DateTime end, Guid? excludedSessionId = null)
    {
        IQueryable<Session> query = _context.Sessions.Where(session => session.SpaceId == spaceId);

        if (excludedSessionId.HasValue)
        {
            query = query.Where(session => session.Id != excludedSessionId.Value);
        }

        bool conflictExists = await query.AnyAsync(session => session.DateDebut < end && session.DateFin > start);

        return !conflictExists;
    }

    public async Task DeleteAsync(Guid id)
    {
        await Task.CompletedTask;
        throw new NotSupportedException("Physical delete is not allowed.");
    }
}
