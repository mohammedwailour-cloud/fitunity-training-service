using Training.Domain.Enums;

namespace Training.Application.Calendar.DTOs;

public class CalendarItemDto
{
    public Guid SessionId { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public string SpaceName { get; set; } = string.Empty;
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public SessionType Type { get; set; }
}
