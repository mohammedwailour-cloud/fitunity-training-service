using Training.Application.Common.Interfaces;
using Training.Application.Coachs.Interfaces;
using Training.Application.Exceptions;
using Training.Application.Sessions.DTOs;
using Training.Application.Sessions.Interfaces;
using Training.Application.Sessions.Mappers;
using Training.Domain.Events;
using Training.Domain.Exceptions;

namespace Training.Application.Sessions.UseCases
{
    public class CreateSessionUseCase
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly ICoachRepository _coachRepository;
        private readonly IEventPublisher _eventPublisher;

        public CreateSessionUseCase(
            ISessionRepository sessionRepository,
            ICoachRepository coachRepository,
            IEventPublisher eventPublisher)
        {
            _sessionRepository = sessionRepository;
            _coachRepository = coachRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<SessionResponse> Execute(CreateSessionRequest request)
        {
            await EnsureCoachMatchesActivityAsync(request.CoachId, request.ActivityId);

            var session = SessionMapper.ToEntity(request);

            await _sessionRepository.AddAsync(session);

            var domainEvent = new SessionCreatedEvent(session.Id);
            await _eventPublisher.PublishAsync(domainEvent);

            return SessionMapper.ToResponse(session);
        }

        private async Task EnsureCoachMatchesActivityAsync(Guid? coachId, Guid? activityId)
        {
            if (!coachId.HasValue)
                return;

            var coach = await _coachRepository.GetByIdAsync(coachId.Value);

            if (coach == null)
                throw new CoachNotFoundException(coachId.Value);

            if (!activityId.HasValue || coach.ActivityId != activityId.Value)
                throw new CoachActivityMismatchException(coach.Id, activityId ?? Guid.Empty);
        }
    }
}
