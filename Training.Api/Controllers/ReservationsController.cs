using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.Application.Common.DTOs;
using Training.Application.Reservations.DTOs;
using Training.Domain.Entities;

namespace Training.Api.Controllers
{
    [ApiController]
    [Authorize]
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
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Reserve(CreateReservationRequest request)
        {
            ReservationResponse result = await _reserveSessionUseCase.ExecuteAsync(request);
            return Ok(result);
        }

        [HttpPatch("{id}/confirm")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmReservation(Guid id)
        {
            await _confirmReservationUseCase.ExecuteAsync(id);
            return NoContent();
        }

        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> CancelReservation(Guid id)
        {
            await _cancelReservationUseCase.ExecuteAsync(id);
            return NoContent();
        }

        [HttpGet("me")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyReservations()
        {
            var result = await _getReservationsByUserUseCase.ExecuteForCurrentUserAsync();
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReservations([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            PagedResult<Reservation> pagedResult =
                await _getReservationsPagedUseCase.ExecuteAsync(page, pageSize);
            return Ok(pagedResult);
        }
    }
}
