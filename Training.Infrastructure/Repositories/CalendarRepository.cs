using Microsoft.EntityFrameworkCore;
using Training.Application.Calendar.DTOs;
using Training.Application.Calendar.Interfaces;
using Training.Infrastructure.Persistence;

namespace Training.Infrastructure.Repositories;

public class CalendarRepository : ICalendarRepository
{
    private const string UnknownSpaceName = "Unknown Space";

    private readonly TrainingDbContext _context;

    public CalendarRepository(TrainingDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CalendarItemDto>> GetUserCalendarAsync(Guid userId)
    {
        List<CalendarItemDto> sessionItems = await (
            from reservation in _context.Reservations.AsNoTracking()
            join session in _context.Sessions.AsNoTracking()
                on reservation.SessionId equals session.Id
            join activity in _context.Activities.AsNoTracking()
                on session.ActivityId equals activity.Id into activityGroup
            from activity in activityGroup.DefaultIfEmpty()
            join space in _context.Spaces.AsNoTracking()
                on session.SpaceId equals space.Id into spaceGroup
            from space in spaceGroup.DefaultIfEmpty()
            where reservation.UserId == userId
            select new CalendarItemDto
            {
                Id = session.Id,
                Type = "session",
                Title = activity != null ? activity.Nom : "Session",
                DateDebut = session.DateDebut,
                DateFin = session.DateFin,
                SpaceName = space != null ? space.Name : UnknownSpaceName,
                ActivityName = activity != null ? activity.Nom : null
            })
            .ToListAsync();

        List<CalendarItemDto> eventItems = await _context.Events
            .AsNoTracking()
            .Include(ev => ev.Space)
            .Select(ev => new CalendarItemDto
            {
                Id = ev.Id,
                Type = "event",
                Title = ev.Titre,
                DateDebut = ev.DateDebut,
                DateFin = ev.DateFin,
                SpaceName = ev.Space != null ? ev.Space.Name : UnknownSpaceName,
                ActivityName = null
            })
            .ToListAsync();

        List<CalendarItemDto> calendarItems = sessionItems
            .Concat(eventItems)
            .OrderBy(item => item.DateDebut)
            .ToList();

        return calendarItems;
    }
}
