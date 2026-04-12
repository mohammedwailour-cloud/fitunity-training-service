using Microsoft.EntityFrameworkCore;
using Training.Application.Coachs.Interfaces;
using Training.Domain.Entities;
using Training.Infrastructure.Persistence;

namespace Training.Infrastructure.Repositories;

public class CoachRepository : ICoachRepository
{
    private readonly TrainingDbContext _context;

    public CoachRepository(TrainingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Coach coach)
    {
        await _context.Coaches.AddAsync(coach);
        await _context.SaveChangesAsync();
    }

    public async Task<Coach> GetByIdAsync(Guid id)
    {
        return await _context.Coaches
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Coach>> GetAllAsync(int page, int pageSize)
    {
        return await _context.Coaches
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task UpdateAsync(Coach coach)
    {
        _context.Coaches.Update(coach);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var coach = await _context.Coaches.FindAsync(id);

        if (coach == null)
            throw new Exception("Coach not found");

        _context.Coaches.Remove(coach);
        await _context.SaveChangesAsync();
    }
}