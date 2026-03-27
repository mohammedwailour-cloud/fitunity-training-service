using Microsoft.EntityFrameworkCore;
using Training.Application.Reservations.Interfaces;
using Training.Domain.Entities;
using Training.Infrastructure.Persistence;

public class ReservationRepository : IReservationRepository
{
    private readonly TrainingDbContext _context;

    public ReservationRepository(TrainingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Reservation reservation)
    {
        await _context.Reservations.AddAsync(reservation);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Reservation reservation)
    {
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();
    }

    public async Task<Reservation?> GetByIdAsync(Guid id)
    {
        return await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Reservation>> GetBySessionIdAsync(Guid sessionId)
    {
        return await _context.Reservations
            .Where(r => r.SessionId == sessionId)
            .ToListAsync();
    }
    public async Task<IEnumerable<Reservation>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Reservations
            .Where(r => r.UserId == userId)
            .ToListAsync();
    }
    public async Task<(IEnumerable<Reservation>, int totalCount)> GetPagedAsync(int page, int pageSize)
    {
        var totalCount = await _context.Reservations.CountAsync();

        var skip = (page - 1) * pageSize;

        var reservations = await _context.Reservations
            .OrderByDescending(r => r.DateReservation)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return (reservations, totalCount);
    }
}