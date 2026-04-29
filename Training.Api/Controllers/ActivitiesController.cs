using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.Application.Activities.DTOs;
using Training.Application.Activities.UseCases;
using Training.Application.Activities.UseCases.Training.Application.Activities.UseCases;
using Training.Application.Common.DTOs;

namespace Training.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/activities")]
    public class ActivitiesController : ControllerBase
    {
        private readonly CreateActivityUseCase _createActivityUseCase;
        private readonly GetActivityByIdUseCase _getActivityByIdUseCase;
        private readonly GetActivitiesUseCase _getActivitiesUseCase;
        private readonly UpdateActivityUseCase _updateActivityUseCase;
        private readonly DeleteActivityUseCase _deleteActivityUseCase;

        public ActivitiesController(
            CreateActivityUseCase createActivityUseCase,
            GetActivityByIdUseCase getActivityByIdUseCase,
            GetActivitiesUseCase getActivitiesUseCase,
            UpdateActivityUseCase updateActivityUseCase,
            DeleteActivityUseCase deleteActivityUseCase)
        {
            _createActivityUseCase = createActivityUseCase;
            _getActivityByIdUseCase = getActivityByIdUseCase;
            _getActivitiesUseCase = getActivitiesUseCase;
            _updateActivityUseCase = updateActivityUseCase;
            _deleteActivityUseCase = deleteActivityUseCase;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ActivityResponse>> CreateActivity([FromBody] CreateActivityRequest request)
        {
            ActivityResponse result = await _createActivityUseCase.ExecuteAsync(request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ActivityResponse>> GetActivityById(Guid id)
        {
            ActivityResponse? result = await _getActivityByIdUseCase.ExecuteAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ActivityResponse>>> GetActivities([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            PagedResult<ActivityResponse> result = await _getActivitiesUseCase.ExecuteAsync(page, pageSize);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ActivityResponse>> UpdateActivity(Guid id, [FromBody] UpdateActivityRequest request)
        {
            ActivityResponse? result = await _updateActivityUseCase.ExecuteAsync(id, request);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            bool success = await _deleteActivityUseCase.ExecuteAsync(id);

            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
