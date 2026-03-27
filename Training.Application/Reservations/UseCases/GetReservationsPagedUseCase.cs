using Training.Application.Common.DTOs;
using Training.Application.Reservations.Interfaces;
using Training.Domain.Entities;

public class GetReservationsPagedUseCase
{
    private readonly IReservationRepository _reservationRepository;

    public GetReservationsPagedUseCase(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<PagedResult<Reservation>> ExecuteAsync(int page, int pageSize)
    {
        var (reservations, totalCount) =
            await _reservationRepository.GetPagedAsync(page, pageSize);

        return new PagedResult<Reservation>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            Data = reservations
        };
    }
}