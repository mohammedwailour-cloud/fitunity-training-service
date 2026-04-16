using Training.Application.Exceptions;
using Training.Application.Spaces.DTOs;
using Training.Application.Spaces.Interfaces;
using Training.Application.Spaces.Mappers;

namespace Training.Application.Spaces.UseCases;

public class CreateSpaceUseCase
{
    private readonly ISpaceRepository _spaceRepository;

    public CreateSpaceUseCase(ISpaceRepository spaceRepository)
    {
        _spaceRepository = spaceRepository;
    }

    public async Task<SpaceResponse> ExecuteAsync(CreateSpaceRequest request)
    {
        bool isCodeUnique = await _spaceRepository.IsCodeUniqueAsync(request.Code);

        if (!isCodeUnique)
        {
            throw new SpaceCodeAlreadyExistsException(request.Code);
        }

        Training.Domain.Entities.Space space = SpaceMapper.ToEntity(request);
        Training.Domain.Entities.Space createdSpace = await _spaceRepository.AddAsync(space);

        return SpaceMapper.ToResponse(createdSpace);
    }
}
