using Training.Application.Exceptions;
using Training.Application.Spaces.DTOs;
using Training.Application.Spaces.Interfaces;
using Training.Application.Spaces.Mappers;
using Training.Domain.Entities;

namespace Training.Application.Spaces.UseCases;

public class GetSpaceByIdUseCase
{
    private readonly ISpaceRepository _spaceRepository;

    public GetSpaceByIdUseCase(ISpaceRepository spaceRepository)
    {
        _spaceRepository = spaceRepository;
    }

    public async Task<SpaceResponse> ExecuteAsync(Guid id)
    {
        Space? space = await _spaceRepository.GetByIdAsync(id);

        if (space == null)
        {
            throw new SpaceNotFoundException(id);
        }

        return SpaceMapper.ToResponse(space);
    }
}
