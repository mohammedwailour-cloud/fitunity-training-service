using Training.Application.Common.Interfaces;
using Training.Application.Reservations.DTOs;
using Training.Application.Reservations.Interfaces;
using Training.Application.Reservations.Mappers;
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

    public async Task<List<ReservationResponse>> ExecuteAsync(Guid userId)
    {
        IEnumerable<Reservation> reservations = await _reservationRepository.GetByUserIdAsync(userId);
        return ReservationMapper.ToResponseList(reservations);
    }

    public async Task<List<ReservationResponse>> ExecuteForCurrentUserAsync()
    {
        IEnumerable<Reservation> reservations = await _reservationRepository.GetByUserIdAsync(_userContext.UserId);
        return ReservationMapper.ToResponseList(reservations);
    }
}
