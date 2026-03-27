using Training.Domain.Enums;

namespace Training.Application.Sessions.DTOs;

public class CreateSessionRequest
{
    public SessionType Type { get; set; }

    public DateTime DateDebut { get; set; }

    public DateTime DateFin { get; set; }

    public int? Capacite { get; set; }

    public decimal? Prix { get; set; }

    public bool AbonnementRequis { get; set; }

    public Guid? ActivityId { get; set; }

    public Guid? CoachId { get; set; }

    public Guid? EventId { get; set; }
}