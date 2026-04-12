namespace Training.Domain.Entities;

public class Event
{
    public Guid Id { get; private set; }
    public string Titre { get; private set; }
    public string? Description { get; private set; }
    public DateTime Date { get; private set; }
    public int Capacite { get; private set; }

    private readonly List<Session> _sessions = new();
    public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();

    private Event() { }

    public Event(string titre, string? description, DateTime date, int capacite)
    {
        if (string.IsNullOrWhiteSpace(titre))
            throw new ArgumentException("Titre obligatoire");

        if (date < DateTime.UtcNow)
            throw new ArgumentException("La date ne peut pas être dans le passé");

        if (capacite <= 0)
            throw new ArgumentException("La capacité doit être positive");

        Id = Guid.NewGuid();
        Titre = titre;
        Description = description;
        Date = date;
        Capacite = capacite;
    }

    public void Update(string titre, string? description, DateTime date, int capacite)
    {
        if (string.IsNullOrWhiteSpace(titre))
            throw new ArgumentException("Titre obligatoire");

        if (date < DateTime.UtcNow)
            throw new ArgumentException("La date ne peut pas être dans le passé");

        if (capacite <= 0)
            throw new ArgumentException("La capacité doit être positive");

        Titre = titre;
        Description = description;
        Date = date;
        Capacite = capacite;
    }

    public void AjouterSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        _sessions.Add(session);
    }
}