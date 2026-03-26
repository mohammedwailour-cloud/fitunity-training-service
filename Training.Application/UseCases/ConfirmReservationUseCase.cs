using Training.Application.Interfaces;
using Training.Domain.Events;
using Training.Domain.Exceptions;

public class ConfirmReservationUseCase
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IEventPublisher _eventPublisher;

    public ConfirmReservationUseCase(IReservationRepository reservationRepository, IEventPublisher eventPublisher)
    {
        _reservationRepository = reservationRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task ExecuteAsync(Guid reservationId)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId);

        if (reservation == null)
            throw new ReservationNotFoundException(reservationId);

        reservation.Confirm();

        await _reservationRepository.UpdateAsync(reservation);
        // Publish Event
        var domainEvent = new ReservationConfirmedEvent(
            reservation.Id,
            reservation.SessionId,
            reservation.UserId
        );

        await _eventPublisher.PublishAsync(domainEvent);

    }
}