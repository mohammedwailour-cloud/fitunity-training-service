

namespace Training.Application.DTOs
{
    public class CreateReservationRequest
    {
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
    }
}
