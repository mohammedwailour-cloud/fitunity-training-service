using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Training.Domain.Events
{
    public class ReservationCreatedEvent
    {
        public Guid ReservationId { get; }
        public Guid SessionId { get; }
        public Guid UserId { get; }

        public ReservationCreatedEvent(Guid reservationId, Guid sessionId, Guid userId)
        {
            ReservationId = reservationId;
            SessionId = sessionId;
            UserId = userId;
        }
    }
}
