using Microsoft.EntityFrameworkCore;
using Training.Application.Calendar.DTOs;
using Training.Application.Calendar.Interfaces;
using Training.Infrastructure.Persistence;

namespace Training.Infrastructure.Repositories;

public class CalendarRepository : ICalendarRepository
{
    private const string UnknownActivityName = "Unknown Activity";
    private const string UnknownSpaceName = "Unknown Space";

    private readonly TrainingDbContext _context;

    public CalendarRepository(TrainingDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CalendarItemDto>> GetUserCalendarAsync(Guid userId)
    {
        List<CalendarItemDto> calendarItems = await (
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
            orderby session.DateDebut
            select new CalendarItemDto
            {
                SessionId = session.Id,
                ActivityName = activity != null ? activity.Nom : UnknownActivityName,
                SpaceName = space != null ? space.Name : UnknownSpaceName,
                DateDebut = session.DateDebut,
                DateFin = session.DateFin,
                Type = session.Type
            })
            .ToListAsync();

        return calendarItems;
    }
}
