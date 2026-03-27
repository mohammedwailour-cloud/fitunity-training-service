using Training.Application.Common.Interfaces;
using Training.Application.Sessions.DTOs;
using Training.Application.Sessions.Interfaces;
using Training.Application.Sessions.Mappers;
using Training.Domain.Entities;
using Training.Domain.Events;

namespace Training.Application.Sessions.UseCases
{
    public class CreateSessionUseCase
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IEventPublisher _eventPublisher;

        public CreateSessionUseCase(ISessionRepository sessionRepository, IEventPublisher eventPublisher)
        {
            _sessionRepository = sessionRepository;
            _eventPublisher = eventPublisher;
        }


        public async Task<SessionResponse> Execute(CreateSessionRequest request)
        {
            var session = SessionMapper.ToEntity(request);

            await _sessionRepository.AddAsync(session);
            // Domain Event
            var domainEvent = new SessionCreatedEvent(session.Id);
            await _eventPublisher.PublishAsync(domainEvent);

            return SessionMapper.ToResponse(session);
        }
    }
}