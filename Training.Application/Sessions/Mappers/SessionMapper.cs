using Training.Application.Sessions.DTOs;
using Training.Domain.Entities;

namespace Training.Application.Sessions.Mappers;

public static class SessionMapper
{
    public static Session ToEntity(CreateSessionRequest dto)
    {
        return new Session(
            dto.Type,
            dto.DateDebut,
            dto.DateFin,
            dto.Capacite,
            dto.Prix,
            dto.AbonnementRequis,
            dto.SpaceId,
            dto.ActivityId,
            dto.CoachId,
            dto.EventId
        );
    }

    public static SessionResponse ToResponse(Session session, Space? space = null)
    {
        return new SessionResponse
        {
            Id = session.Id,
            Type = session.Type,
            DateDebut = session.DateDebut,
            DateFin = session.DateFin,
            Capacite = session.Capacite,
            Prix = session.Prix,
            AbonnementRequis = session.AbonnementRequis,
            SpaceId = session.SpaceId,
            SpaceName = space?.Name ?? string.Empty,
            SpaceType = space?.Type ?? default,
            ActivityId = session.ActivityId,
            CoachId = session.CoachId,
            EventId = session.EventId
        };
    }
}
