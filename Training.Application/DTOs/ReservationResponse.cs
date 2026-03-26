

using Training.Domain.Enums;

namespace Training.Application.DTOs
{
    public class ReservationResponse
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public DateTime DateReservation { get; set; }
        public ReservationStatus Status { get; set; }
    }
}
