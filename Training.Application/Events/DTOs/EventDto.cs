using Training.Domain.Enums;

namespace Training.Application.Events.DTOs;

public class EventDto
{
    public Guid Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public int Capacite { get; set; }
    public Guid SpaceId { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public SpaceType SpaceType { get; set; }
}
