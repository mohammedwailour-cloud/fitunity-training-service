using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using Training.Application.Spaces.Interfaces;
using Training.Domain.Entities;
using Training.Infrastructure.Persistence;

namespace Training.Infrastructure.Repositories;

public class SpaceRepository : ISpaceRepository
{
    private readonly TrainingDbContext _context;

    public SpaceRepository(TrainingDbContext context)
    {
        _context = context;
    }

    public async Task<Space> AddAsync(Space space)
    {
        _context.Spaces.Add(space);
        await _context.SaveChangesAsync();
        return space;
    }

    public async Task<Space?> GetByIdAsync(Guid id)
    {
        return await _context.Spaces.FirstOrDefaultAsync(space => space.Id == id);
    }

    public async Task UpdateAsync(Space space)
    {
        _context.Spaces.Update(space);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await Task.CompletedTask;
        throw new NotSupportedException("Physical delete is not allowed. Use soft delete (IsActive).");

        // Pas de suppression réelle
        // historique conservé
        // cohérence avec Session / Event
        // Evite bugs futurs
    }
    

    public async Task<(IEnumerable<Space> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        IQueryable<Space> query = _context.Spaces.Where(space => space.IsActive);
        int totalCount = await query.CountAsync();
        List<Space> items = await query
            .OrderBy(space => space.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludedSpaceId = null)
    {
        string normalizedCode = code.Trim().ToUpper();
        IQueryable<Space> query = _context.Spaces.Where(space => space.Code == normalizedCode && space.IsActive == true);

        if (excludedSpaceId.HasValue)
        {
            query = query.Where(space => space.Id != excludedSpaceId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<bool> IsSpaceAvailableAsync(
        Guid spaceId,
        DateTime start,
        DateTime end,
        Guid? excludedSessionId = null,
        Guid? excludedEventId = null)
    {
        IQueryable<Session> sessionQuery = _context.Sessions.Where(session => session.SpaceId == spaceId);

        if (excludedSessionId.HasValue)
        {
            sessionQuery = sessionQuery.Where(session => session.Id != excludedSessionId.Value);
        }

        bool sessionConflictExists = await sessionQuery.AnyAsync(session => session.DateDebut < end && session.DateFin > start);

        if (sessionConflictExists)
        {
            return false;
        }

        IQueryable<Event> eventQuery = _context.Events.Where(ev => ev.SpaceId == spaceId);

        if (excludedEventId.HasValue)
        {
            eventQuery = eventQuery.Where(ev => ev.Id != excludedEventId.Value);
        }

        bool eventConflictExists = await eventQuery.AnyAsync(ev => ev.DateDebut < end && ev.DateFin > start);

        return !eventConflictExists;
    }
}
