using System.Data;
using Training.Application.DTOs;
using Training.Application.Interfaces;
using Training.Domain.Entities;
using Training.Domain.Events;
using Training.Domain.Exceptions;
using Training.Application.Exceptions;

public class ReserveSessionUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IEventPublisher _eventPublisher;

    public ReserveSessionUseCase(
        ISessionRepository sessionRepository,
        IReservationRepository reservationRepository,
        IEventPublisher eventPublisher)
    {
        _sessionRepository = sessionRepository;
        _reservationRepository = reservationRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<ReservationResponse> ExecuteAsync(CreateReservationRequest request)
    {
        // 1. vérifier session existe
        var session = await _sessionRepository.GetByIdAsync(request.SessionId);
        if (session == null)
            throw new SessionNotFoundException(request.SessionId);

        // 2. vérifier session pas dans le passé
        if (session.IsInPast())
            throw new InvalidSessionStateException("Cannot reserve a past session");


        // 3. récupérer réservations existantes
        var reservations = (await _reservationRepository
            .GetBySessionIdAsync(request.SessionId))
            .ToList();

        // 4. vérifier capacité
        if (session.IsFull(reservations.Count))
            throw new SessionFullException();

        // 5. vérifier doublon
        if (reservations.Any(r => r.UserId == request.UserId))
            throw new DuplicateReservationException();

        // 6. créer réservation
        var reservation = new Reservation(
            request.SessionId,
            request.UserId
        );

        // Domain Event
        await _reservationRepository.AddAsync(reservation);
        var domainEvent = new ReservationCreatedEvent(
         reservation.Id,
         reservation.SessionId,
         reservation.UserId
        );

        await _eventPublisher.PublishAsync(domainEvent);

        // 7. retour DTO
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