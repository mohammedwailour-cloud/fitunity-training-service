using Training.Application.Exceptions;
using Training.Application.Spaces.DTOs;
using Training.Application.Spaces.Interfaces;
using Training.Application.Spaces.Mappers;
using Training.Domain.Entities;

namespace Training.Application.Spaces.UseCases;

public class UpdateSpaceUseCase
{
    private readonly ISpaceRepository _spaceRepository;

    public UpdateSpaceUseCase(ISpaceRepository spaceRepository)
    {
        _spaceRepository = spaceRepository;
    }

    public async Task<SpaceResponse> ExecuteAsync(Guid id, UpdateSpaceRequest request)
    {
        Space? space = await _spaceRepository.GetByIdAsync(id);

        if (space == null)
        {
            throw new SpaceNotFoundException(id);
        }

        bool isCodeUnique = await _spaceRepository.IsCodeUniqueAsync(request.Code, id);

        if (!isCodeUnique)
        {
            throw new SpaceCodeAlreadyExistsException(request.Code);
        }

        space.Update(
            request.Name,
            request.Code,
            request.Description,
            request.Type,
            request.Capacity,
            request.SupportsSeatManagement,
            request.IsActive);

        await _spaceRepository.UpdateAsync(space);

        return SpaceMapper.ToResponse(space);
    }
}
