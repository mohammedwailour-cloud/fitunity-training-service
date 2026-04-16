using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.Application.Calendar.DTOs;
using Training.Application.Calendar.UseCases;

namespace Training.Api.Controllers;

[ApiController]
[Authorize(Roles = "User,Admin")]
[Route("api/[controller]")]
public class CalendarController : ControllerBase
{
    private readonly GetUserCalendarUseCase _getUserCalendarUseCase;

    public CalendarController(GetUserCalendarUseCase getUserCalendarUseCase)
    {
        _getUserCalendarUseCase = getUserCalendarUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        IEnumerable<CalendarItemDto> calendar = await _getUserCalendarUseCase.ExecuteAsync();
        return Ok(calendar);
    }
}
