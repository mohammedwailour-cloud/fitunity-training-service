using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Training.Application.Events.DTOs;
using Training.Application.Events.UseCases;

namespace Training.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly CreateEventUseCase _createEventUseCase;
        private readonly GetEventByIdUseCase _getEventByIdUseCase;
        private readonly GetEventsUseCase _getEventsUseCase;
        private readonly UpdateEventUseCase _updateEventUseCase;
        private readonly DeleteEventUseCase _deleteEventUseCase;

        public EventsController(
            CreateEventUseCase createEventUseCase,
            GetEventByIdUseCase getEventByIdUseCase,
            GetEventsUseCase getEventsUseCase,
            UpdateEventUseCase updateEventUseCase,
            DeleteEventUseCase deleteEventUseCase)
        {
            _createEventUseCase = createEventUseCase;
            _getEventByIdUseCase = getEventByIdUseCase;
            _getEventsUseCase = getEventsUseCase;
            _updateEventUseCase = updateEventUseCase;
            _deleteEventUseCase = deleteEventUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
        {
            var id = await _createEventUseCase.Execute(dto);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var ev = await _getEventByIdUseCase.Execute(id);
            return Ok(ev);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var events = await _getEventsUseCase.Execute(page, pageSize);
            return Ok(events);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventDto dto)
        {
            await _updateEventUseCase.Execute(id, dto);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _deleteEventUseCase.Execute(id);
            return NoContent();
        }
    }
}
