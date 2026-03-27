using Training.Application.Reservations.Interfaces;
using Training.Domain.Entities;

public class GetReservationsBySessionUseCase
{
    private readonly IReservationRepository _reservationRepository;

    public GetReservationsBySessionUseCase(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<IEnumerable<Reservation>> ExecuteAsync(Guid sessionId)
    {
        return await _reservationRepository.GetBySessionIdAsync(sessionId);
    }
}