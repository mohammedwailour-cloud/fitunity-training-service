using Training.Application.Common.Interfaces;
using Training.Application.Exceptions;
using Training.Application.Reservations.DTOs;
using Training.Application.Reservations.Interfaces;
using Training.Application.Sessions.Interfaces;
using Training.Domain.Entities;
using Training.Domain.Events;
using Training.Domain.Exceptions;

public class ReserveSessionUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUserContext _userContext;

    public ReserveSessionUseCase(
        ISessionRepository sessionRepository,
        IReservationRepository reservationRepository,
        IEventPublisher eventPublisher,
        IUserContext userContext)
    {
        _sessionRepository = sessionRepository;
        _reservationRepository = reservationRepository;
        _eventPublisher = eventPublisher;
        _userContext = userContext;
    }

    public async Task<ReservationResponse> ExecuteAsync(CreateReservationRequest request)
    {
        var userId = _userContext.UserId;

        var session = await _sessionRepository.GetByIdAsync(request.SessionId);
        if (session == null)
            throw new SessionNotFoundException(request.SessionId);

        if (session.IsInPast())
            throw new InvalidSessionStateException();

        var reservations = (await _reservationRepository
            .GetBySessionIdAsync(request.SessionId))
            .ToList();

        if (session.IsFull(reservations.Count))
            throw new SessionFullException();

        if (reservations.Any(r => r.UserId == userId))
            throw new DuplicateReservationException();

        var reservation = new Reservation(
            userId,
            request.SessionId
        );

        await _reservationRepository.AddAsync(reservation);

        var domainEvent = new ReservationCreatedEvent(
            reservation.Id,
            reservation.SessionId,
            reservation.UserId
        );

        await _eventPublisher.PublishAsync(domainEvent);

        return new ReservationResponse
        {
            Id = reservation.Id,
            SessionId = reservation.SessionId,
            UserId = reservation.UserId,
            DateReservation = reservation.DateReservation,
            Status = reservation.Status
        };
    }
}
