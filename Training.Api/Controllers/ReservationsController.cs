using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.Application.Reservations.DTOs;

namespace Training.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,User,Coach")]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly ReserveSessionUseCase _reserveSessionUseCase;
        private readonly ConfirmReservationUseCase _confirmReservationUseCase;
        private readonly CancelReservationUseCase _cancelReservationUseCase;
        private readonly GetReservationsPagedUseCase _getReservationsPagedUseCase;
        private readonly GetReservationsByUserUseCase _getReservationsByUserUseCase;

        public ReservationsController(
            ReserveSessionUseCase reserveSessionUseCase,
            ConfirmReservationUseCase confirmReservationUseCase,
            CancelReservationUseCase cancelReservationUseCase,
            GetReservationsPagedUseCase getReservationsPagedUseCase,
            GetReservationsByUserUseCase getReservationsByUserUseCase)
        {
            _reserveSessionUseCase = reserveSessionUseCase;
            _confirmReservationUseCase = confirmReservationUseCase;
            _cancelReservationUseCase = cancelReservationUseCase;
            _getReservationsPagedUseCase = getReservationsPagedUseCase;
            _getReservationsByUserUseCase = getReservationsByUserUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> Reserve(CreateReservationRequest request)
        {
            var result = await _reserveSessionUseCase.ExecuteAsync(request);

            return Ok(result);
        }

        [HttpPatch("{id}/confirm")]
        public async Task<IActionResult> ConfirmReservation(Guid id)
        {
            await _confirmReservationUseCase.ExecuteAsync(id);
            return NoContent();
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelReservation(Guid id)
        {
            await _cancelReservationUseCase.ExecuteAsync(id);
            return NoContent();
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyReservations()
        {
            var result = await _getReservationsByUserUseCase.ExecuteForCurrentUserAsync();
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetReservations(
         [FromQuery] int page = 1,
         [FromQuery] int pageSize = 10)
        {
            var result = await _getReservationsPagedUseCase.ExecuteAsync(page, pageSize);
            return Ok(result);
        }
    }
}
