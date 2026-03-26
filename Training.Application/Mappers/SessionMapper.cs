using Training.Application.DTOs;
using Training.Domain.Entities;

namespace Training.Application.Mappers;

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
            dto.ActivityId,
            dto.CoachId,
            dto.EventId
        );
    }

    public static SessionResponse ToResponse(Session session)
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
            ActivityId = session.ActivityId,
            CoachId = session.CoachId,
            EventId = session.EventId
        };
    }
}