using Training.Application.Reservations.DTOs;
using Training.Domain.Entities;

namespace Training.Application.Reservations.Mappers;

public static class ReservationMapper
{
    public static ReservationResponse ToResponse(Reservation reservation)
    {
        return new ReservationResponse
        {
            Id = reservation.Id,
            SessionId = reservation.SessionId,
            UserId = reservation.UserId,
            DateReservation = reservation.DateReservation,
            Status = reservation.Status
        };
    }

    public static List<ReservationResponse> ToResponseList(IEnumerable<Reservation> reservations)
    {
        return reservations.Select(ToResponse).ToList();
    }
}
