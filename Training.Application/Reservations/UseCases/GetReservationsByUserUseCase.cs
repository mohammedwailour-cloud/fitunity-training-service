using Training.Application.Common.Interfaces;
using Training.Application.Reservations.Interfaces;
using Training.Domain.Entities;

public class GetReservationsByUserUseCase
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IUserContext _userContext;

    public GetReservationsByUserUseCase(IReservationRepository reservationRepository, IUserContext userContext)
    {
        _reservationRepository = reservationRepository;
        _userContext = userContext;
    }

    public async Task<IEnumerable<Reservation>> ExecuteAsync(Guid userId)
    {
        return await _reservationRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<Reservation>> ExecuteForCurrentUserAsync()
    {
        return await _reservationRepository.GetByUserIdAsync(_userContext.UserId);
    }
}
