using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Training.Application.Reservations.DTOs;
using Training.Application.Sessions.DTOs;
using Training.Application.Sessions.UseCases;

namespace Training.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly GetSessionUseCase _getSessionUseCase;
        private readonly CreateSessionUseCase _createSessionUseCase;
        private readonly GetAllSessionsUseCase _getAllSessionsUseCase;
        private readonly GetReservationsBySessionUseCase _getReservationsBySessionUseCase;
        private readonly GetSessionsPagedUseCase _getSessionsPagedUseCase;
        private readonly UpdateSessionUseCase _updateSessionUseCase;
        private readonly DeleteSessionUseCase _deleteSessionUseCase;

        public SessionsController(
            GetSessionUseCase getSessionUseCase,
            CreateSessionUseCase createSessionUseCase,
            GetAllSessionsUseCase getAllSessionsUseCase,
            GetReservationsBySessionUseCase getReservationsBySessionUseCase,
            GetSessionsPagedUseCase getSessionsPagedUseCase,
            UpdateSessionUseCase updateSessionUseCase,
            DeleteSessionUseCase deleteSessionUseCase)
        {
            _getSessionUseCase = getSessionUseCase;
            _createSessionUseCase = createSessionUseCase;
            _getAllSessionsUseCase = getAllSessionsUseCase;
            _getReservationsBySessionUseCase = getReservationsBySessionUseCase;
            _getSessionsPagedUseCase = getSessionsPagedUseCase;
            _updateSessionUseCase = updateSessionUseCase;
            _deleteSessionUseCase = deleteSessionUseCase;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Coach")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            SessionResponse result = await _createSessionUseCase.Execute(request);
            return CreatedAtAction(nameof(GetSession), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSession(Guid id)
        {
            SessionResponse? result = await _getSessionUseCase.Execute(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("{sessionId}/reservations")]
        public async Task<IActionResult> GetReservationsBySession(Guid sessionId)
        {
            List<ReservationResponse> reservations = await _getReservationsBySessionUseCase.ExecuteAsync(sessionId);
            return Ok(reservations);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetSessions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _getSessionsPagedUseCase.ExecuteAsync(page, pageSize);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Coach")]
        public async Task<ActionResult<SessionResponse>> UpdateSession(Guid id, UpdateSessionRequest request)
        {
            SessionResponse? result = await _updateSessionUseCase.ExecuteAsync(id, request);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteSession(Guid id)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                statusCode = StatusCodes.Status501NotImplemented,
                error = "not_implemented",
                message = "Deleting sessions is not implemented."
            });
        }
    }
}
