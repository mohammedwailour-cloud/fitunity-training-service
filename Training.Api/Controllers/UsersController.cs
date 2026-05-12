using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.Application.Reservations.DTOs;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly GetReservationsByUserUseCase _getReservationsByUserUseCase;

    public UsersController(GetReservationsByUserUseCase getReservationsByUserUseCase)
    {
        _getReservationsByUserUseCase = getReservationsByUserUseCase;
    }

    [HttpGet("{userId}/reservations")]
    public async Task<IActionResult> GetReservationsByUser(Guid userId)
    {
        List<ReservationResponse> reservations = await _getReservationsByUserUseCase.ExecuteAsync(userId);
        return Ok(reservations);
    }
}
