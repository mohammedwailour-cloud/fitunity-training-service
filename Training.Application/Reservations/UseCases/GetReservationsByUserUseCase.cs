using Training.Application.Reservations.Interfaces;
using Training.Domain.Entities;

public class GetReservationsByUserUseCase
{
    private readonly IReservationRepository _reservationRepository;

    public GetReservationsByUserUseCase(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<IEnumerable<Reservation>> ExecuteAsync(Guid userId)
    {
        return await _reservationRepository.GetByUserIdAsync(userId);
    }
}