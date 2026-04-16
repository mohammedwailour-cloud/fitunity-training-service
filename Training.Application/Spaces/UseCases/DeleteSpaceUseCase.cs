using Training.Application.Exceptions;
using Training.Application.Spaces.Interfaces;
using Training.Domain.Entities;

namespace Training.Application.Spaces.UseCases;

public class DeleteSpaceUseCase
{
    private readonly ISpaceRepository _spaceRepository;

    public DeleteSpaceUseCase(ISpaceRepository spaceRepository)
    {
        _spaceRepository = spaceRepository;
    }

    public async Task ExecuteAsync(Guid id)
    {
        Space? space = await _spaceRepository.GetByIdAsync(id);

        if (space == null)
        {
            throw new SpaceNotFoundException(id);
        }

        space.Update(
            space.Name,
            space.Code,
            space.Description,
            space.Type,
            space.Capacity,
            space.SupportsSeatManagement,
            false);

        await _spaceRepository.UpdateAsync(space);
    }
}
