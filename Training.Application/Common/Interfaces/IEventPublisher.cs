using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Training.Application.Common.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T domainEvent);
        // après on utilisera KAFKA ou RabbitMQ 
    }
}
