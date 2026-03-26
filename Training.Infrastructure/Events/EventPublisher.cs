using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Training.Application.Interfaces;

namespace Training.Infrastructure.Events
{
    public class EventPublisher : IEventPublisher
    {
        public Task PublishAsync<T>(T domainEvent)
        {
            Console.WriteLine($"Event published: {typeof(T).Name}");
            return Task.CompletedTask;
        }
    }
}
