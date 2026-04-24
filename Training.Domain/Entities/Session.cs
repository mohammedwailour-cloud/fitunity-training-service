using Training.Domain.Enums;
using Training.Domain.Exceptions;

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
    public bool IsOpenSession { get; private set; }

    public Guid SpaceId { get; private set; }
    public Space? Space { get; private set; }

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
        Guid spaceId,
        Guid? activityId = null,
        Guid? coachId = null,
        Guid? eventId = null,
        bool isOpenSession = false)
    {
        if (dateFin <= dateDebut)
            throw new InvalidSessionDatesException();

        if (capacite.HasValue && capacite.Value <= 0)
            throw new InvalidSessionCapacityException();

        if (prix.HasValue && prix.Value < 0)
            throw new InvalidSessionPriceException();

        if (spaceId == Guid.Empty)
            throw new InvalidSessionSpaceException();

        if (isOpenSession && type != SessionType.Open)
            throw new InvalidOpenSessionException();

        if (isOpenSession && (coachId.HasValue || !activityId.HasValue || spaceId == Guid.Empty))
            throw new InvalidOpenSessionException();

        Id = Guid.NewGuid();
        Type = type;
        DateDebut = dateDebut;
        DateFin = dateFin;
        Capacite = capacite;
        Prix = prix;
        AbonnementRequis = abonnementRequis;
        IsOpenSession = isOpenSession;
        SpaceId = spaceId;
        ActivityId = activityId;
        CoachId = coachId;
        EventId = eventId;
    }

    public void Update(
        SessionType type,
        DateTime dateDebut,
        DateTime dateFin,
        int? capacite,
        decimal? prix,
        bool abonnementRequis,
        Guid spaceId,
        Guid? activityId,
        Guid? coachId,
        bool isOpenSession)
    {
        if (IsInPast())
            throw new InvalidSessionStateException();

        if (dateFin <= dateDebut)
            throw new InvalidSessionDatesException();

        if (capacite.HasValue && capacite.Value <= 0)
            throw new InvalidSessionCapacityException();

        if (prix.HasValue && prix.Value < 0)
            throw new InvalidSessionPriceException();

        if (spaceId == Guid.Empty)
            throw new InvalidSessionSpaceException();

        if (isOpenSession && type != SessionType.Open)
            throw new InvalidOpenSessionException();

        if (isOpenSession && (coachId.HasValue || !activityId.HasValue || spaceId == Guid.Empty))
            throw new InvalidOpenSessionException();

        if (capacite.HasValue && capacite.Value < _reservations.Count)
            throw new SessionCapacityConflictException();

        Type = type;
        DateDebut = dateDebut;
        DateFin = dateFin;
        Capacite = capacite;
        Prix = prix;
        AbonnementRequis = abonnementRequis;
        IsOpenSession = isOpenSession;
        SpaceId = spaceId;
        ActivityId = activityId;
        CoachId = coachId;
    }

    public void AjouterReservation(Guid userId)
    {
        if (IsInPast())
            throw new InvalidSessionStateException();

        if (IsFull(_reservations.Count))
            throw new SessionFullException();

        if (_reservations.Any(r => r.UserId == userId))
            throw new DuplicateReservationException();

        Reservation reservation = new(userId, Id);
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
