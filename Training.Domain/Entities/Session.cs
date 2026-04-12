using Training.Domain.Enums;

namespace Training.Domain.Entities;

public class Session
{
    public Guid Id { get; private set; }

    public SessionType Type { get; private set; }

    public DateTime DateDebut { get; private set; }
    public DateTime DateFin { get; private set; }

    public int? Capacite { get; private set; }
    public decimal? Prix { get; private set; }

    public bool AbonnementRequis { get; private set; }

    public Guid? ActivityId { get; private set; }
    public ActivitySportive? Activity { get; private set; }

    public Guid? CoachId { get; private set; }
    public Coach? Coach { get; private set; }

    public Guid? EventId { get; private set; }
    public Event? Event { get; private set; }

    private readonly List<Reservation> _reservations = new();
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    private Session() { }

    public Session(
        SessionType type,
        DateTime dateDebut,
        DateTime dateFin,
        int? capacite,
        decimal? prix,
        bool abonnementRequis,
        Guid? activityId = null,
        Guid? coachId = null,
        Guid? eventId = null)
    {
        if (dateFin <= dateDebut)
            throw new ArgumentException("Date invalide");

        if (capacite.HasValue && capacite.Value <= 0)
            throw new ArgumentException("Capacité invalide");

        if (prix.HasValue && prix.Value < 0)
            throw new ArgumentException("Prix invalide");

        Id = Guid.NewGuid();
        Type = type;
        DateDebut = dateDebut;
        DateFin = dateFin;
        Capacite = capacite;
        Prix = prix;
        AbonnementRequis = abonnementRequis;
        ActivityId = activityId;
        CoachId = coachId;
        EventId = eventId;
    }

    public void Update(
        DateTime dateDebut,
        DateTime dateFin,
        int? capacite,
        decimal? prix,
        bool abonnementRequis,
        Guid? coachId)
    {
        if (dateFin <= dateDebut)
            throw new ArgumentException("Date invalide");

        if (capacite.HasValue && capacite.Value <= 0)
            throw new ArgumentException("Capacité invalide");

        if (prix.HasValue && prix.Value < 0)
            throw new ArgumentException("Prix invalide");

        DateDebut = dateDebut;
        DateFin = dateFin;
        Capacite = capacite;
        Prix = prix;
        AbonnementRequis = abonnementRequis;
        CoachId = coachId;
    }

    public void AjouterReservation(Guid userId)
    {
        if (IsInPast())
            throw new InvalidOperationException("Impossible de réserver une session passée");

        if (IsFull(_reservations.Count))
            throw new InvalidOperationException("Session complète");

        if (_reservations.Any(r => r.UserId == userId))
            throw new InvalidOperationException("Utilisateur déjà inscrit");

        var reservation = new Reservation(userId, Id);
        _reservations.Add(reservation);
    }

    public bool IsFull(int currentReservations)
    {
        if (!Capacite.HasValue)
            return false;

        return currentReservations >= Capacite.Value;
    }

    public bool IsInPast()
    {
        return DateDebut < DateTime.UtcNow;
    }
}