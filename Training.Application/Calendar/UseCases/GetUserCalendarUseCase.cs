using Training.Application.Calendar.DTOs;
using Training.Application.Calendar.Interfaces;
using Training.Application.Common.Interfaces;

namespace Training.Application.Calendar.UseCases;

public class GetUserCalendarUseCase
{
    private readonly IUserContext _userContext;
    private readonly ICalendarRepository _calendarRepository;

    public GetUserCalendarUseCase(IUserContext userContext, ICalendarRepository calendarRepository)
    {
        _userContext = userContext;
        _calendarRepository = calendarRepository;
    }

    public async Task<IEnumerable<CalendarItemDto>> ExecuteAsync()
    {
        Guid userId = _userContext.UserId;
        IEnumerable<CalendarItemDto> calendarItems = await _calendarRepository.GetUserCalendarAsync(userId);
        return calendarItems;
    }
}
