using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        var reservations = await _getReservationsByUserUseCase.ExecuteAsync(userId);
        return Ok(reservations);
    }
}
