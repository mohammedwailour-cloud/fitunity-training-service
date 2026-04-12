namespace Training.Application.Events.DTOs;

public class EventDto
{
    public Guid Id { get; set; }
    public string Titre { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public int Capacite { get; set; }
}