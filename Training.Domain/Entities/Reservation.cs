using Training.Domain.Enums;

namespace Training.Domain.Entities;

public class Reservation
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    public Guid SessionId { get; private set; }
    public Session Session { get; private set; }

    public DateTime DateReservation { get; private set; }
    public ReservationStatus Status { get; private set; }

    private Reservation() { }

    public Reservation(Guid userId, Guid sessionId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required");

        if (sessionId == Guid.Empty)
            throw new ArgumentException("SessionId is required");

        Id = Guid.NewGuid();
        UserId = userId;
        SessionId = sessionId;
        DateReservation = DateTime.UtcNow;
        Status = ReservationStatus.EnAttente;
    }

    public void Confirm()
    {
        if (Status == ReservationStatus.Annulee)
            throw new InvalidOperationException("Impossible de confirmer une réservation annulée");

        Status = ReservationStatus.Confirmee;
    }

    public void Cancel()
    {
        Status = ReservationStatus.Annulee;
    }

    public void UpdateStatus(ReservationStatus status)
    {
        Status = status;
    }
}