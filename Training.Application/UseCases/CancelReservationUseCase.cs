using Training.Application.Interfaces;
using Training.Domain.Events;

public class CancelReservationUseCase
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IEventPublisher _eventPublisher;

    public CancelReservationUseCase(IReservationRepository reservationRepository, IEventPublisher eventPublisher)
    {
        _reservationRepository = reservationRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task ExecuteAsync(Guid reservationId)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId);

        if (reservation == null)
            throw new Exception("Reservation not found");

        reservation.Cancel();
        // Domain Event
        await _reservationRepository.UpdateAsync(reservation);

        var domainEvent = new ReservationCancelledEvent(
            reservation.Id,
            reservation.SessionId,
            reservation.UserId
        );

        await _eventPublisher.PublishAsync(domainEvent);

        await _reservationRepository.UpdateAsync(reservation);
    }
}