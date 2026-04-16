using Training.Application.Spaces.DTOs;
using Training.Domain.Entities;

namespace Training.Application.Spaces.Mappers;

public static class SpaceMapper
{
    public static Space ToEntity(CreateSpaceRequest request)
    {
        return new Space(
            request.Name,
            request.Code,
            request.Description,
            request.Type,
            request.Capacity,
            request.SupportsSeatManagement,
            request.IsActive);
    }

    public static SpaceResponse ToResponse(Space space)
    {
        return new SpaceResponse
        {
            Id = space.Id,
            Name = space.Name,
            Code = space.Code,
            Description = space.Description,
            Type = space.Type,
            Capacity = space.Capacity,
            SupportsSeatManagement = space.SupportsSeatManagement,
            IsActive = space.IsActive
        };
    }
}
