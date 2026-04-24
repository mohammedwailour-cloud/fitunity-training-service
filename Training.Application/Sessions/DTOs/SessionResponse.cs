using Training.Domain.Enums;

namespace Training.Application.Sessions.DTOs;

public class SessionResponse
{
    public Guid Id { get; set; }

    public SessionType Type { get; set; }

    public DateTime DateDebut { get; set; }

    public DateTime DateFin { get; set; }

    public int? Capacite { get; set; }

    public decimal? Prix { get; set; }

    public bool AbonnementRequis { get; set; }
    public bool IsOpenSession { get; set; }
    public Guid SpaceId { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public SpaceType SpaceType { get; set; }
    public Guid? ActivityId { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public Guid? CoachId { get; set; }
    public Guid? EventId { get; set; }
}
