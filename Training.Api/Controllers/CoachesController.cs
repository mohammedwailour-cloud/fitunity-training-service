using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.Application.Coachs.DTOs;
using Training.Application.Coachs.UseCases;

namespace Training.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class CoachesController : ControllerBase
{
    private readonly CreateCoachUseCase _createCoach;
    private readonly GetCoachByIdUseCase _getCoachById;
    private readonly GetCoachesUseCase _getCoaches;
    private readonly DeleteCoachUseCase _deleteCoach;
    private readonly UpdateCoachUseCase _updateCoach;

    public CoachesController(
        CreateCoachUseCase createCoach,
        GetCoachByIdUseCase getCoachById,
        GetCoachesUseCase getCoaches,
        DeleteCoachUseCase deleteCoach,
        UpdateCoachUseCase updateCoach)
    {
        _createCoach = createCoach;
        _getCoachById = getCoachById;
        _getCoaches = getCoaches;
        _deleteCoach = deleteCoach;
        _updateCoach = updateCoach;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCoachDto dto)
    {
        var id = await _createCoach.Execute(dto);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var coach = await _getCoachById.Execute(id);
        return Ok(coach);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
    {
        var coaches = await _getCoaches.Execute(page, pageSize);
        return Ok(coaches);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deleteCoach.Execute(id);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCoachDto dto)
    {
        var updatedCoach = await _updateCoach.ExecuteAsync(id, dto);

        if (updatedCoach == null)
            return NotFound();

        return Ok(updatedCoach);
    }
}
