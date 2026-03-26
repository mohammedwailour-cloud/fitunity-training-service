namespace Training.Domain.Exceptions;

public class ReservationNotFoundException : Exception
{
    public ReservationNotFoundException(Guid id)
        : base($"Reservation {id} not found")
    {
    }
}
