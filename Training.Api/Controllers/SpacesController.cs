using Microsoft.AspNetCore.Mvc;
using Training.Application.Common.DTOs;
using Training.Application.Spaces.DTOs;
using Training.Application.Spaces.UseCases;

namespace Training.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpacesController : ControllerBase
{
    private readonly CreateSpaceUseCase _createSpaceUseCase;
    private readonly UpdateSpaceUseCase _updateSpaceUseCase;
    private readonly GetSpaceByIdUseCase _getSpaceByIdUseCase;
    private readonly GetSpacesUseCase _getSpacesUseCase;
    private readonly DeleteSpaceUseCase _deleteSpaceUseCase;

    public SpacesController(
        CreateSpaceUseCase createSpaceUseCase,
        UpdateSpaceUseCase updateSpaceUseCase,
        GetSpaceByIdUseCase getSpaceByIdUseCase,
        GetSpacesUseCase getSpacesUseCase,
        DeleteSpaceUseCase deleteSpaceUseCase)
    {
        _createSpaceUseCase = createSpaceUseCase;
        _updateSpaceUseCase = updateSpaceUseCase;
        _getSpaceByIdUseCase = getSpaceByIdUseCase;
        _getSpacesUseCase = getSpacesUseCase;
        _deleteSpaceUseCase = deleteSpaceUseCase;
    }

    [HttpPost]
    public async Task<ActionResult<SpaceResponse>> Create([FromBody] CreateSpaceRequest request)
    {
        SpaceResponse result = await _createSpaceUseCase.ExecuteAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SpaceResponse>> Update(Guid id, [FromBody] UpdateSpaceRequest request)
    {
        SpaceResponse result = await _updateSpaceUseCase.ExecuteAsync(id, request);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SpaceResponse>> GetById(Guid id)
    {
        SpaceResponse result = await _getSpaceByIdUseCase.ExecuteAsync(id);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<SpaceResponse>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        PagedResult<SpaceResponse> result = await _getSpacesUseCase.ExecuteAsync(page, pageSize);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deleteSpaceUseCase.ExecuteAsync(id);
        return NoContent();
    }
}
