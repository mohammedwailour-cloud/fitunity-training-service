using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Training.Domain.Events
{
    public class SessionCreatedEvent
    {
        public Guid SessionId { get; }

        public SessionCreatedEvent(Guid sessionId)
        {
            SessionId = sessionId;
        }
    }
}
