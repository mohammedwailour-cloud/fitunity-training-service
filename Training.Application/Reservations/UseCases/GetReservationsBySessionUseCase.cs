using Training.Application.Reservations.DTOs;
using Training.Application.Reservations.Interfaces;
using Training.Application.Reservations.Mappers;
using Training.Domain.Entities;

public class GetReservationsBySessionUseCase
{
    private readonly IReservationRepository _reservationRepository;

    public GetReservationsBySessionUseCase(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<List<ReservationResponse>> ExecuteAsync(Guid sessionId)
    {
        IEnumerable<Reservation> reservations = await _reservationRepository.GetBySessionIdAsync(sessionId);
        return ReservationMapper.ToResponseList(reservations);
    }
}
