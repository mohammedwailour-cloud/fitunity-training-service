using Training.Domain.Exceptions;

namespace Training.Domain.Entities;

public class Event
{
    public Guid Id { get; private set; }
    public string Titre { get; private set; }
    public string? Description { get; private set; }
    public DateTime DateDebut { get; private set; }
    public DateTime DateFin { get; private set; }
    public int Capacite { get; private set; }
    public Guid SpaceId { get; private set; }
    public Space? Space { get; private set; }

    private readonly List<Session> _sessions = new();
    public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();

    private Event() { }

    public Event(string titre, string? description, DateTime dateDebut, DateTime dateFin, int capacite, Guid spaceId)
    {
        if (string.IsNullOrWhiteSpace(titre))
            throw new InvalidEventTitleException();

        if (dateDebut < DateTime.UtcNow)
            throw new InvalidEventDatesException();

        if (dateFin <= dateDebut)
            throw new InvalidEventDatesException();

        if (capacite <= 0)
            throw new InvalidEventCapacityException();

        if (spaceId == Guid.Empty)
            throw new InvalidEventSpaceException();

        Id = Guid.NewGuid();
        Titre = titre;
        Description = description;
        DateDebut = dateDebut;
        DateFin = dateFin;
        Capacite = capacite;
        SpaceId = spaceId;
    }

    public void Update(string titre, string? description, DateTime dateDebut, DateTime dateFin, int capacite, Guid spaceId)
    {
        if (string.IsNullOrWhiteSpace(titre))
            throw new InvalidEventTitleException();

        if (dateDebut < DateTime.UtcNow)
            throw new InvalidEventDatesException();

        if (dateFin <= dateDebut)
            throw new InvalidEventDatesException();

        if (capacite <= 0)
            throw new InvalidEventCapacityException();

        if (spaceId == Guid.Empty)
            throw new InvalidEventSpaceException();

        Titre = titre;
        Description = description;
        DateDebut = dateDebut;
        DateFin = dateFin;
        Capacite = capacite;
        SpaceId = spaceId;
    }

    public void AjouterSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        _sessions.Add(session);
    }
}
