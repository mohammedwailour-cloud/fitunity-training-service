using Training.Application.Common.Interfaces;
using Training.Application.Reservations.Interfaces;
using Training.Domain.Entities;
using Training.Domain.Events;
using Training.Domain.Exceptions;

public class CancelReservationUseCase
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUserContext _userContext;

    public CancelReservationUseCase(
        IReservationRepository reservationRepository,
        IEventPublisher eventPublisher,
        IUserContext userContext)
    {
        _reservationRepository = reservationRepository;
        _eventPublisher = eventPublisher;
        _userContext = userContext;
    }

    public async Task ExecuteAsync(Guid reservationId)
    {
        Reservation? reservation = await _reservationRepository.GetByIdAsync(reservationId);

        if (reservation == null)
            throw new ReservationNotFoundException(reservationId);

        if (reservation.UserId != _userContext.UserId && _userContext.Role != "Admin")
            throw new InvalidReservationUserException();

        reservation.Cancel();
        await _reservationRepository.UpdateAsync(reservation);

        var domainEvent = new ReservationCancelledEvent(
            reservation.Id,
            reservation.SessionId,
            reservation.UserId
        );

        await _eventPublisher.PublishAsync(domainEvent);
    }
}
