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
            dto.EventId,
            dto.IsOpenSession
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
            IsOpenSession = session.IsOpenSession,
            SpaceId = session.SpaceId,
            SpaceName = space?.Name ?? session.Space?.Name ?? string.Empty,
            SpaceType = space?.Type ?? session.Space?.Type ?? default,
            ActivityId = session.ActivityId,
            ActivityName = session.Activity?.Nom ?? string.Empty,
            CoachId = session.CoachId,
            EventId = session.EventId
        };
    }
}
