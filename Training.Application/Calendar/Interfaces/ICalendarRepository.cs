using Training.Application.Calendar.DTOs;

namespace Training.Application.Calendar.Interfaces;

public interface ICalendarRepository
{
    Task<IEnumerable<CalendarItemDto>> GetUserCalendarAsync(Guid userId);
}
