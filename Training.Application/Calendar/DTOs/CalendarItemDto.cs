namespace Training.Application.Calendar.DTOs;

public class CalendarItemDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public string? ActivityName { get; set; }
}
