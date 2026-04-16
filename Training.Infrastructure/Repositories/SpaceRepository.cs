using Microsoft.EntityFrameworkCore;
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
        Space? space = await _context.Spaces.FindAsync(id);

        if (space != null)
        {
            _context.Spaces.Remove(space);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<(IEnumerable<Space> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        IQueryable<Space> query = _context.Spaces.AsQueryable();
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
        IQueryable<Space> query = _context.Spaces.Where(space => space.Code == normalizedCode);

        if (excludedSpaceId.HasValue)
        {
            query = query.Where(space => space.Id != excludedSpaceId.Value);
        }

        return !await query.AnyAsync();
    }
}
